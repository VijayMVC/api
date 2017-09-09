using ConnectCMS.Models;
using ConnectCMS.Repositories.Caching;
using Kent.Boogaart.KBCsv;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceBusLibrary;
using MonsciergeServiceUtilities;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class SmsRepository : ChildRepository
	{
		public SmsRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public SmsRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IQueryable<SMSStockMessage> GetSMSStockMessages( int userId, int hotelId )
		{
			return
				Rp.ExecuteAction(
					() =>
						ProxylessContext.SMSStockMessages.Where(
							m => m.FKHotel == hotelId ) );
		}

		public IQueryable<SMSStockMessage> GetSmsStockMessage( int userId, int messageId )
		{
			return
				Rp.ExecuteAction(
					() =>
						ProxylessContext.SMSStockMessages.Where(
							m => m.PKID == messageId ) );
		}

		public void DeleteSMSStockMessage( int userId, int messageId )
		{
			var msg = GetSmsStockMessage( userId, messageId ).FirstOrDefault();
			if( msg != null )
			{
				ProxylessContext.SMSStockMessages.Remove( msg );
				ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			}
		}

		public SMSStockMessage SaveSMSStockMessage( int userId, SMSStockMessage message )
		{
			var result = RootRepository.HotelRepository.CheckHotelPermission( userId, message.FKHotel );
			if( result.Result == PermissionResults.Authorized )
			{
				if( message.PKID != 0 )
				{
					var msg = Rp.ExecuteAction( () => ProxylessContext.SMSStockMessages.FirstOrDefault( m => m.PKID == message.PKID ) );
					if( msg != null )
					{
						msg.Name = message.Name;
						msg.Text = message.Text;
						ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
						return msg;
					}
				}
				else
				{
					var msg = new SMSStockMessage
					{
						Name = message.Name,
						Text = message.Text,
						FKHotel = message.FKHotel
					};
					ProxylessContext.SMSStockMessages.Add( msg );
					ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
					return msg;
				}
			}
			return null;
		}

		public IQueryable<SMSJob> GetSMSJobs( int hotelId )
		{
			var jobs = Rp.ExecuteAction( () => ProxylessContext.SMSJobs.Include( sj => sj.SMSTasks ).Where( sj => sj.FKHotel == hotelId ) );
			return jobs;
		}

		public IQueryable<SMSTask> GetSMSTask( int taskId )
		{
			var task = Rp.ExecuteAction( () => ProxylessContext.SMSTasks.Where( st => st.PKID == taskId ) );
			return task;
		}

		public IQueryable<SMSJob> GetSMSJob( int jobId )
		{
			var job = Rp.ExecuteAction( () => ProxylessContext.SMSJobs.Where( st => st.PKID == jobId ) );
			return job;
		}

		public SMSTask CancelSMSTask( int userId, int taskId )
		{
			var task = GetSMSTask( taskId ).Include( t => t.SMSJob ).FirstOrDefault();
			if( task == null )
				return null;
			task.Status = SMSTaskStatus.CANCELLED;
			task.StatusLastUpdated = DateTime.UtcNow;
			ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			return task;
		}

		public PermissionResult CheckSMSTaskPermission( int userId, int taskId )
		{
			var hotelId = GetSMSTask( taskId ).Select( x => x.SMSJob.FKHotel ).FirstOrDefault();
			var hasHotelPermission = RootRepository.HotelRepository.CheckHotelPermission( userId, hotelId );
			return hasHotelPermission;
		}

		public PermissionResult CheckSMSJobPermission( int userId, int jobId )
		{
			var hotelId = GetSMSJob( jobId ).Select( x => x.FKHotel ).FirstOrDefault();
			var hasHotelPermission = RootRepository.HotelRepository.CheckHotelPermission( userId, hotelId );
			return hasHotelPermission;
		}

		public SMSJob CancelSMSJob( int userId, int jobId )
		{
			if( CheckSMSJobPermission( userId, jobId ).Result != PermissionResults.Authorized )
				return null;
			var job = GetSMSJob( jobId ).Include( x => x.SMSTasks );
			var tasks = job.SelectMany( x => x.SMSTasks.Where( t => t.Status == SMSTaskStatus.SCHEDULED ) ).ToList();
			if( tasks.Count <= 0 )
				return null;
			tasks.ForEach( t =>
			{
				t.Status = SMSTaskStatus.CANCELLED;
				t.StatusLastUpdated = DateTime.UtcNow;
			} );
			ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			SmsTaskTopicSender.GetHandle().NotifySmsTaskInserted();
			return job.FirstOrDefault();
		}

		public SMSTask RetrySMSTask( int userId, int taskId )
		{
			if( CheckSMSTaskPermission( userId, taskId ).Result != PermissionResults.Authorized )
				return null;
			var task = GetSMSTask( taskId ).Include( t => t.SMSJob ).Include( t => t.SMSJob.SMSJobCallbacks ).FirstOrDefault();
			if( task == null )
				return null;
			var nowWithBuffer = DateTime.UtcNow.AddMinutes( -1 );
			var scheduledTasks =
				GetSMSJobs( task.SMSJob.FKHotel )
					.SelectMany(
						j => j.SMSTasks.Where( t => t.Status == SMSTaskStatus.SCHEDULED && t.RunAt > nowWithBuffer ) ).ToList();

			var hotel = RootRepository.HotelRepository.GetHotelQuery( task.SMSJob.FKHotel ).Include( x => x.HotelDetail.SMSWelcomeRequestType.SMSNumbers ).FirstOrDefault();
			var numbers = hotel.HotelDetail.SMSWelcomeRequestType.SMSNumbers.Select( n => n.PhoneNumber ).ToList();

			SortedSet<SmsTimeSlot> timeslots = null;
			var timeslot = GetNextAvailableTaskTimeslot( numbers, DateTime.UtcNow, task.Message.Length, scheduledTasks, ref timeslots );

			task.RunAt = timeslot.Item2;
			task.FromPhone = timeslot.Item1;
			task.Status = SMSTaskStatus.SCHEDULED;
			task.StatusMessage = null;
			task.MessageId = null;
			task.StatusLastUpdated = DateTime.UtcNow;
			if( task.SMSJob.SMSJobCallbacks.All( x => x.Url != "https://Connect.Monscierge.com/ConnectCMS/Request/NotifyTaskChanged" ) )
				task.SMSJob.SMSJobCallbacks.Add( new SMSJobCallback { Url = "https://Connect.Monscierge.com/ConnectCMS/Request/NotifyTaskChanged" } );

			ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			SmsTaskTopicSender.GetHandle().NotifySmsTaskInserted();
			return task;
		}

		public SMSJob SaveSMSJob( SMSJob job, string auditUser )
		{
			var numbers =
				Rp.ExecuteAction(
					() =>
						ProxylessContext.HotelDetails.Where( h => h.PKID == job.FKHotel )
							.SelectMany( h => h.SMSWelcomeRequestType.SMSNumbers.Select( n => n.PhoneNumber ) ) );
			SortedSet<SmsTimeSlot> timeslots = null;
			var taskStart = DateTime.UtcNow.AddMinutes( -1 );
			var tasks =
				Rp.ExecuteAction( () => ProxylessContext.SMSTasks.Where( x => numbers.Contains( x.FromPhone ) && x.RunAt > taskStart && x.Status == SMSTaskStatus.SCHEDULED ) ).ToList();
			var existing = Rp.ExecuteAction( () => ProxylessContext.SMSJobs.Where( x => x.PKID == job.PKID ) ).FirstOrDefault();
			if( existing == null )
			{
				job.CreatedOn = DateTime.UtcNow;
				job.Status = SMSJobStatus.INCOMPLETE;
				var availableNumbers = numbers.ToList();
				job.SMSTasks.ForEach( t =>
				{
					t.Status = SMSTaskStatus.SCHEDULED;
					t.Message = t.Message.Replace( "{FirstName}", t.FirstName ).Replace( "{LastName}", t.LastName );
					var timeslot = GetNextAvailableTaskTimeslot( availableNumbers, t.RunAt.ToUniversalTime(), t.Message.Length, tasks,
						ref timeslots );
					t.FromPhone = timeslot.Item1;
					t.RunAt = timeslot.Item2;
					t.StatusLastUpdated = DateTime.UtcNow;
				} );
			}
			else
			{
				return null;
			}

			if( job.SMSJobCallbacks.All( x => x.Url != "https://Connect.Monscierge.com/ConnectCMS/Request/NotifyTaskChanged" ) )
				job.SMSJobCallbacks.Add( new SMSJobCallback { Url = "https://Connect.Monscierge.com/ConnectCMS/Request/NotifyTaskChanged" } );

			ProxylessContext.SMSJobs.Add( job );
			ProxylessContext.LogValidationFailSaveChanges( string.Format( auditUser ) );
			SmsTaskTopicSender.GetHandle().NotifySmsTaskInserted();
			return job;
		}

		public List<SMSTask> GetSMSTasksFromCsvBlob( string path, int hotelId )
		{
			var bytes = Utilities.ReadAzureBlob( path );
			var stream = new MemoryStream( bytes );
			return GetSMSTasksFromStream( stream, hotelId );
		}

		public List<SMSTask> GetSMSTasksFromStream( Stream stream, int hotelId )
		{
			var list = new List<SMSTask>();
			var phoneUtil = PhoneNumberUtil.GetInstance();

			var hotelCountryCode = RootRepository.HotelRepository.GetHotelQuery( hotelId ).Include( x => x.HotelDetail ).Select( x => x.HotelDetail.ISOCountryCode ).FirstOrDefault();

			using( var reader = new CsvReader( stream ) )
			{
				while( reader.HasMoreRecords )
				{
					var record = reader.ReadDataRecord();

					if( record.Count >= 3 )
					{
						var number = string.Empty;
						try
						{
							var recStr = new string( record[ 2 ].Where<char>( c => char.IsDigit( c ) || c == '+' ).ToArray() );
							if( !string.IsNullOrWhiteSpace( recStr ) )
							{
								var cCode = string.IsNullOrEmpty( hotelCountryCode ) ? "US" : hotelCountryCode;
								var phoneNumber = phoneUtil.Parse( recStr, cCode );
								if( phoneNumber == null || !phoneUtil.IsValidNumber( phoneNumber ) )
								{
									var withPlus = "+" + recStr;
									phoneNumber = phoneUtil.Parse( withPlus, cCode );
									if( phoneNumber == null || !phoneUtil.IsValidNumber( phoneNumber ) )
										throw new Exception( "invalid number" );
								}

								number = phoneUtil.Format( phoneNumber, PhoneNumberFormat.E164 );
							}
						}
						catch( Exception )
						{
							number = string.Empty;
						}
						finally
						{
							var ue = new SMSTask()
							{
								FirstName = record[ 0 ],
								LastName = record[ 1 ],
								ToPhone = number
							};
							if( !string.IsNullOrWhiteSpace( ue.ToPhone ) && list.All( x => x.ToPhone != ue.ToPhone ) )
							{
								list.Add( ue );
							}
						}
					}
				}
			}

			return list;
		}

		public static Tuple<string, DateTime> GetNextAvailableTaskTimeslot( List<string> numbers, DateTime preferredStart, int messageLength, List<SMSTask> tasks, ref SortedSet<SmsTimeSlot> timeslots )
		{
			if( timeslots == null )
			{
				timeslots = new SortedSet<SmsTimeSlot>( Comparer<SmsTimeSlot>.Create( ( a, b ) =>
				{
					var cmp = DateTime.Compare( a.Item2, b.Item2 );
					if( cmp == 0 )
						return String.Compare( a.Item1, b.Item1, StringComparison.InvariantCulture );
					return cmp;
				} ) );
				foreach( var number in numbers )
				{
					var nmb = number;
					var numberGroup = tasks.Where( x => x.RunAt > preferredStart.AddMinutes( -1 ) && x.FromPhone == nmb ).OrderBy( x => x.RunAt ).ToList();
					if( numberGroup.Any() )
					{
						for( var n = 0; n < numberGroup.Count(); n++ )
						{
							var current = numberGroup.ElementAt( n );
							var currentFrom = current.RunAt;
							var currentTo = current.RunAt.AddSeconds( 12 + Math.Ceiling( ( double )current.Message.Length / 160 ) );

							var previous = numberGroup.ElementAtOrDefault( n - 1 );
							var previousTo = previous == null ? preferredStart : previous.RunAt.AddSeconds( 12 + Math.Ceiling( ( double )previous.Message.Length / 160 ) );

							var next = numberGroup.ElementAtOrDefault( n + 1 );
							var nextFrom = next == null ? DateTime.MaxValue : next.RunAt;

							if( previous == null )
							{
								if( previousTo < currentFrom )
								{
									timeslots.Add( new SmsTimeSlot( nmb, preferredStart, preferredStart.Add( currentFrom.Subtract( previousTo ) ) ) );
								}
							}

							if( nextFrom.Subtract( currentTo ).TotalSeconds > 13 )
								timeslots.Add( new SmsTimeSlot( nmb, currentTo, currentTo.Add( nextFrom.Subtract( currentTo ) ) ) );
						}
					}
					else
					{
						timeslots.Add( new SmsTimeSlot( nmb, preferredStart, DateTime.MaxValue ) );
					}
				}
			}

			var messageDuration = TimeSpan.FromSeconds( ( 12 + Math.Ceiling( ( double )messageLength / 160 ) ) );

			var first = timeslots.First( x => x.End >= preferredStart.Add( messageDuration ) );
			timeslots.Remove( first );

			//if timeslots are split, ignore the one prior to preferredStart
			var startTime = new DateTime( Math.Max( preferredStart.Ticks, first.Start.Ticks ) );

			var remainder = first.End - startTime.Add( messageDuration );
			if( remainder > TimeSpan.FromSeconds( 12 ) )
			{
				timeslots.Add( new SmsTimeSlot(
					first.Item1,
					startTime.Add( messageDuration ),
					first.End ) );
			}

			return new Tuple<string, DateTime>( first.Item1, startTime );
		}

		public class SmsTimeSlot : Tuple<string, DateTime, DateTime>
		{
			public SmsTimeSlot( string phoneNumber, DateTime start, DateTime end )
				: base( phoneNumber, start, end )
			{ }

			public string PhoneNumber { get { return Item1; } }

			public DateTime Start { get { return Item2; } }

			public DateTime End { get { return Item3; } }
		}
	}
}