#region using

using ConnectCMS.Models.Events;
using ConnectCMS.Repositories.Caching;
using Glimpse.Core.ClientScript;
using Kendo.Mvc.UI;
using Kent.Boogaart.KBCsv;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using MonsciergeSFWrapper.SF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using Telerik.Web.UI;

#endregion using

namespace ConnectCMS.Repositories
{
	public class EventRepository : ChildRepository
	{
		public EventRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public EventRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp,
			ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IEnumerable<EventResourceModel> LoadEventLocations( int deviceId )
		{
			var result = new List<EventResourceModel>();

			using( var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				Rp.ExecuteAction( () =>
				{
					RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );

					result = ( from el in context.EventLocations
							   where el.FKDevice == deviceId && el.IsActive
							   orderby el.Name
							   select el ).Select(
							x =>
								new EventResourceModel
								{
									Name = x.Name,
									PKID = x.PKID,
									ResourceType = "Location",
									DeviceId = x.Device.PKID
								} )
						.ToList();
				} );
			}

			return result;
		}

		public IEnumerable<EventResourceModel> LoadEventGroups( int deviceId )
		{
			var result = new List<EventResourceModel>();

			using( var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				Rp.ExecuteAction( () =>
				{
					RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );

					result = ( from eg in context.EventGroups
							   where eg.FKDevice == deviceId && eg.IsActive
							   orderby eg.Name
							   select eg ).Select(
							x =>
								new EventResourceModel
								{
									Name = x.Name,
									PKID = x.PKID,
									ResourceType = "Group",
									DeviceId = x.Device.PKID
								} )
						.ToList();
				} );
			}

			return result;
		}

		public void EventResourceCreate( DataSourceRequest request, EventResourceModel resource )
		{
			using(
				var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				switch( resource.ResourceType )
				{
					case "Group":
						var group = new EventGroup
						{
							Name = resource.Name,
							FKDevice = resource.DeviceId,
							IsActive = true,
							LastModifiedDateTime = DateTime.Now
						};
						Rp.ExecuteAction( () =>
						{
							var groupRes = context.EventGroups.AddObject( group );
							context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
							resource.PKID = groupRes.PKID;
						} );
						break;

					case "Location":
						var location = new EventLocation
						{
							Name = resource.Name,
							FKDevice = resource.DeviceId,
							IsActive = true,
							LastModifiedDateTime = DateTime.Now
						};
						Rp.ExecuteAction( () =>
						{
							var locationRes = context.EventLocations.AddObject( location );
							context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
							resource.PKID = locationRes.PKID;
						} );
						break;
				}
			}
		}

		public void EventResourceUpdate( DataSourceRequest request, EventResourceModel resource )
		{
			using(
				var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				switch( resource.ResourceType )
				{
					case "Group":
						var group = Rp.ExecuteAction( () => context.EventGroups.SingleOrDefault( g => g.PKID == resource.PKID ) );
						group.Name = resource.Name;
						group.LastModifiedDateTime = DateTime.Now;
						Rp.ExecuteAction(
							() => { context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ); } );
						break;

					case "Location":
						var location = Rp.ExecuteAction( () => context.EventLocations.SingleOrDefault( g => g.PKID == resource.PKID ) );
						location.Name = resource.Name;
						location.LastModifiedDateTime = DateTime.Now;
						Rp.ExecuteAction(
							() => { context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ); } );
						break;
				}
			}
		}

		public void EventResourceDestroy( DataSourceRequest request, EventResourceModel resource )
		{
			using(
				var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				switch( resource.ResourceType )
				{
					case "Group":
						Rp.ExecuteAction( () =>
						{
							var group = context.EventGroups.SingleOrDefault( g => g.PKID == resource.PKID );
							context.EventGroups.Remove( group );
							context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
						} );
						break;

					case "Location":
						Rp.ExecuteAction( () =>
						{
							var location = context.EventLocations.SingleOrDefault( g => g.PKID == resource.PKID );
							context.EventLocations.Remove( location );
							context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
						} );
						break;
				}
			}
		}

		public List<SchedulerEvent> EventSchedulerRead( int deviceId, DateTime firstDate, DateTime lastDate )
		{
			var result = new List<SchedulerEvent>();
			Rp.ExecuteAction( () =>
			{
				var details = Rp.ExecuteAction( () => ( from ed in Context.EventDetails
														where
															ed.FKDevice == deviceId &&
															ed.IsActive &&
															( ( ed.LocalStartDateTime >= firstDate && ed.LocalStartDateTime <= lastDate ) ||
															 ( ed.LocalEndDateTime >= firstDate && ed.LocalEndDateTime <= lastDate ) || !String.IsNullOrEmpty( ed.RecurrenceRule ) )
														orderby ed.LocalStartDateTime, ed.LocalEndDateTime, ed.Name
														select ed ).ToList() );

				details.ForEach( ed =>
				{
					if( !string.IsNullOrEmpty( ed.RecurrenceRule ) &&
						!IsARecurrsiveDay( firstDate, lastDate, ed.RecurrenceRule, "" ) )
						return;

					var data = new SchedulerEvent
					{
						Description = ed.Description,
						End = DateTime.SpecifyKind( ed.LocalEndDateTime, DateTimeKind.Utc ),
						PKID = ed.PKID,
						Start = DateTime.SpecifyKind( ed.LocalStartDateTime, DateTimeKind.Utc ),
						Title = ed.Name,
						DeviceId = deviceId,
						RecurrenceString = ed.RecurrenceRule,
						IsAllDay =
							ed.LocalStartDateTime.AddDays( 1 ) == ed.LocalEndDateTime &&
							ed.LocalStartDateTime.TimeOfDay.Ticks == 0,
						EventGroupId = ed.FKEventGroup
					};

					if( ed.FKEventLocation != null )
						data.EventLocationId = ed.FKEventLocation.Value;

					result.Add( data );
				} );
			} );
			return result;
		}

		public bool IsARecurrsiveDay( DateTime? start, DateTime? end, string recurrenceRule, string recurrenceException )
		{
			return true;
		}

		public void EventSchedulerCreate( SchedulerEvent info, SchedulerEvent evt )
		{
			using(
				var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				RootRepository.SecurityRepository.AssertDeviceAuthorization( evt.DeviceId );

				Rp.ExecuteAction( () =>
				{
					var detail = new EventDetail();

					PopulateEventWithAppointmentData( info, evt, detail, context );

					context.EventDetails.AddObject( detail );
					context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

					evt.PKID = detail.PKID;
				} );
			}
		}

		private void PopulateEventWithAppointmentData( SchedulerEvent schedulerInfo, SchedulerEvent appointmentData,
			EventDetail detail, MonsciergeEntities context )
		{
			var groupId = appointmentData.EventGroupId;
			var locationId = appointmentData.EventLocationId;

			RootRepository.SecurityRepository.AssertDeviceAuthorization( schedulerInfo.DeviceId );
			if( groupId != 0 )
			{
				RootRepository.SecurityRepository.AssertDeviceLinkedObjectAuthorization(
					d => d.EventGroups.Any( eg => eg.PKID == groupId ) );
			}

			if( locationId != 0 )
			{
				RootRepository.SecurityRepository.AssertDeviceLinkedObjectAuthorization(
					d => d.EventLocations.Any( el => el.PKID == locationId ) );
			}

			if( detail.PKID != 0 )
			{
				RootRepository.SecurityRepository.AssertDeviceLinkedObjectAuthorization(
					d => d.EventDetails.Any( ed => ed.PKID == detail.PKID ) );
			}

			detail.FKDevice = schedulerInfo.DeviceId;

			detail.FKEventGroup = groupId;

			detail.FKEventLocation = locationId;

			detail.LocalStartDateTime = appointmentData.Start;
			detail.LocalEndDateTime = appointmentData.End;
			detail.Name = appointmentData.Title;
			detail.Description = appointmentData.Description;

			if( !string.IsNullOrEmpty( appointmentData.RecurrenceRule ) )
			{
				var str = new StringBuilder();
				str.AppendFormat( "DTSTART:{0}\r\n", appointmentData.Start.ToString( "yyyyMMddTHHmmssZ" ) );
				str.AppendFormat( "DTEND:{0}\r\n", appointmentData.End.ToString( "yyyyMMddTHHmmssZ" ) );
				str.AppendFormat( "RRULE:{0}\r\n", appointmentData.RecurrenceRule );
				str.AppendFormat( "EXDATE:{0}\r\n", appointmentData.RecurrenceException );
				detail.RecurrenceRule = str.ToString();
			}

			detail.IsActive = true;
			detail.LastModifiedDateTime = DateTime.Now;
		}

		public void EventSchedulerUpdate( SchedulerEvent info, SchedulerEvent evt )
		{
			using(
				var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				RootRepository.SecurityRepository.AssertDeviceAuthorization( evt.DeviceId );

				Rp.ExecuteAction( () =>
				{
					var detail = context.EventDetails.FirstOrDefault( x => x.PKID == evt.PKID );

					PopulateEventWithAppointmentData( info, evt, detail, context );

					context.EventDetails.AddOrUpdate( detail );
					context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

					if( detail != null )
						evt.PKID = detail.PKID;
				} );
			}
		}

		public void EventSchedulerDestroy( SchedulerEvent evt )
		{
			using(
				var context = new MonsciergeEntities { CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds } )
			{
				RootRepository.SecurityRepository.AssertDeviceAuthorization( evt.DeviceId );

				Rp.ExecuteAction( () =>
				{
					var itemToRemove = context.EventDetails.FirstOrDefault( x => x.PKID == evt.PKID );
					if( itemToRemove == null )
						return;

					context.EventDetails.Remove( itemToRemove );
					context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
				} );
			}
		}

		public List<EventDetail> GetEvents( int? deviceId, string view, DateTime date )
		{
			var fromDate = date.ToUniversalTime().Date;
			var toDate = date.ToUniversalTime().Date.AddDays( 1 );

			var viewType = view.Split( '|' )[ 0 ];
			var viewPort = view.Split( '|' )[ 1 ];
			var parseRecursion = viewType != "scheduler";

			switch( viewPort )
			{
				case "day":
					break;

				case "week":
					fromDate = date.ToUniversalTime().Date.AddDays( -1 * ( int )date.ToUniversalTime().DayOfWeek );
					toDate = fromDate.AddDays( 7 );
					break;

				case "month":
					fromDate = date.ToUniversalTime().Date.AddDays( -1 * ( date.ToUniversalTime().Day - 1 ) );
					toDate = fromDate.AddMonths( 1 );
					break;
			}

			List<EventDetail> result;
			if( deviceId.HasValue )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
				result =
					Rp.ExecuteAction(
						() =>
							ProxylessContext.EventDetails
								.Include( x => x.EventGroup )
								.Include( x => x.EventLocation )
								.Where(
									detail =>
										string.IsNullOrEmpty( detail.RecurrenceRule ) && detail.IsActive && detail.EventGroup.IsActive &&
										detail.EventLocation.IsActive && detail.FKDevice == deviceId &&
										(
										( detail.LocalStartDateTime >= fromDate && detail.LocalStartDateTime <= toDate )
										||
										( detail.LocalEndDateTime >= fromDate && detail.LocalEndDateTime <= toDate )
										||
										( detail.LocalStartDateTime < fromDate && detail.LocalEndDateTime > toDate )

										) )
								.Select( detail => detail )
								.ToList() );
			}
			else
			{
				var user = RootRepository.SecurityRepository.GetLoggedInUser();
				if( user.DefaultReachRole == null ||
					( !user.DefaultReachRole.ManageAssignedEvents && user.DefaultReachRole.Name != "Super Admin" ) )
				{
					throw new Exception( "You are not an event manager, please select a device to view events." );
				}

				result =
					Rp.ExecuteAction(
						() => ( from ed in ProxylessContext.EventDetails.Include( x => x.EventGroup ).Include( x => x.EventLocation )
								where
									string.IsNullOrEmpty( ed.RecurrenceRule ) &&
									ed.IsActive &&
									ed.EventGroup.IsActive &&
									ed.EventLocation.IsActive &&
									ed.EventGroup.EventGroupManagerMaps.Any( x => x.FKContactUser == user.PKID )
									&& ( ( ed.LocalStartDateTime >= fromDate && ed.LocalEndDateTime < toDate ) )
								select ed ) ).ToList();
			}

			if( parseRecursion )
			{
				var recEvents = ProxylessContext.EventDetails
					.Include( x => x.EventGroup )
					.Include( x => x.EventLocation )
					.Where(
						detail =>
							!string.IsNullOrEmpty( detail.RecurrenceRule ) &&
							detail.IsActive &&
							detail.EventGroup.IsActive &&
							detail.EventLocation.IsActive &&
							detail.FKDevice == deviceId )
					.Select( detail => detail );

				foreach( var eventDetail in recEvents )
				{
					RecurrenceRule rRule = null;
					try
					{
						// Note: The try-catch is here because the retards at Telerik wrote a TryParse method
						// that throws an error if it can't parse the recurrence rule - thereby defeating the purpose 
						// of writing a TryParse method in the first place. The overloaded method 
						// TryParse(string, out RecurrenceRule) also throws errors.
						rRule = RecurrenceRule.TryParse( eventDetail.RecurrenceRule );
					}
					catch( Exception ex )
					{
						Logger.AddRayGunData( ex, "eventId", eventDetail.PKID );
						Logger.AddRayGunData( ex, "recurrenceRule", eventDetail.RecurrenceRule );
						Logger.LogException( ex, "Error parsing recurrence rule." );
						continue;
					}

					eventDetail.Occurrences = rRule.Occurrences.Where( x => x >= fromDate && x < toDate ).ToList();

					result.AddRange(
						eventDetail.Occurrences.Select( occurrence => new EventDetail
						{
							EventGroup =
								new EventGroup
								{
									Name = eventDetail.EventGroup.Name,
									PKID = eventDetail.EventGroup.PKID,
									EventAccessCode = eventDetail.EventGroup.EventAccessCode
								},
							EventLocation = new EventLocation { Name = eventDetail.EventLocation.Name, PKID = eventDetail.EventLocation.PKID },
							Name = eventDetail.Name,
							LocalStartDateTime =
								new DateTime( occurrence.Year, occurrence.Month, occurrence.Day, occurrence.Hour, occurrence.Minute,
									occurrence.Second ),
							LocalEndDateTime =
								new DateTime( occurrence.Year, occurrence.Month, occurrence.Day, eventDetail.LocalEndDateTime.Hour,
									eventDetail.LocalEndDateTime.Minute, eventDetail.LocalEndDateTime.Second ),
							IsAllDay = eventDetail.IsAllDay,
							RecurrenceRule = eventDetail.RecurrenceRule,
							FKRecurrenceParentId = eventDetail.PKID,
							IsActive = eventDetail.IsActive,
							PKID = eventDetail.PKID,
							Occurrences = eventDetail.Occurrences,
							StaffOnly = eventDetail.StaffOnly
						} ) );
				}
			}

			result.ForEach( detail =>
			{
				detail.IsAllDay = detail.LocalStartDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) &&
								  detail.LocalEndDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) &&
								  detail.LocalStartDateTime < detail.LocalEndDateTime;
				if( !detail.IsAllDay )
					return;
				detail.LocalStartDateTime = detail.LocalStartDateTime.Date;
				detail.LocalEndDateTime = detail.LocalEndDateTime.Date.AddDays( -1 );
			} );

			if( viewType == "list" )
			{
				return
					result.GroupBy( x => new EventDetailGroupByModel { StartTime = x.LocalStartDateTime, EventGroup = x.EventGroup.Name }, x => x, ( Key, details ) => new { Key, Items = details.ToList() } ).OrderBy( x => x.Key.StartTime ).ThenBy( x => x.Key.EventGroup ).SelectMany( x => x.Items ).ToList();
			}
			if( viewType == "scheduler" )
			{
				var take = viewPort == "day" ? 10 : ( viewPort == "week" ? 5 : 2 );
				var filteredResults =
					result.GroupBy( x => new EventDetailGroupByModel { StartTime = x.LocalStartDateTime.Date, EventGroup = "any" }, x => x,
						( Key, details ) => new { Key = Key.StartTime, Items = details.Take( take ).ToList() } ).OrderBy( x => x.Key ).SelectMany( x => x.Items ).ToList();

				filteredResults.AddRange( ProxylessContext.EventDetails
					.Include( x => x.EventGroup )
					.Include( x => x.EventLocation )
					.Where(
						detail =>
							!string.IsNullOrEmpty( detail.RecurrenceRule ) &&
							detail.IsActive &&
							detail.EventGroup.IsActive &&
							detail.EventLocation.IsActive &&
							detail.FKDevice == deviceId )
					.Select( detail => detail ) );
				return filteredResults;
			}
			return result.OrderBy( x => x.LocalStartDateTime ).ToList();
		}

		public void UpdateTime( EventDetail eventDetail )
		{
			var evt =
				Rp.ExecuteAction( () => ( from e in Context.EventDetails where e.PKID == eventDetail.PKID select e ) )
					.FirstOrDefault();

			if( evt == null )
				throw new InvalidDataException( "The event you are trying to access does not exist" );
			RootRepository.SecurityRepository.AssertDevicePermissions( evt.FKDevice );

			if( eventDetail.IsAllDay )
			{
				eventDetail.LocalStartDateTime = eventDetail.LocalStartDateTime.Date;
				eventDetail.LocalEndDateTime = eventDetail.LocalEndDateTime.Date.AddDays( 1 );
			}

			evt.LocalEndDateTime = eventDetail.LocalEndDateTime;
			evt.LocalStartDateTime = eventDetail.LocalStartDateTime;

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public List<EventLocation> GetEventLocations( int? deviceId )
		{
			List<EventLocation> result;
			if( deviceId.HasValue )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
				result =
					Rp.ExecuteAction(
						() =>
							ProxylessContext.EventLocations
								.Where( location => location.IsActive && location.FKDevice == deviceId ).Select( detail => detail )
								.ToList() );
			}
			else
			{
				result = new List<EventLocation>();
			}

			return result;
		}

		public List<EventGroup> GetEventGroups( int? deviceId )
		{
			IDictionary<int, string> urls = new Dictionary<int, string>();
			IDictionary<int, bool> hasReader = new ConcurrentDictionary<int, bool>();
			List<EventGroup> result;
			if( deviceId.HasValue )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
				result =
					Rp.ExecuteAction(
						() =>
							ProxylessContext.EventGroups
								.Include( group => group.EventGroupManagerMaps )
								.Include( group => group.EventGroupManagerMaps.Select( maps => maps.ContactUser ) )
								.Include( group => group.ReaderboardBackgroundImage )
								.Where( group => group.IsActive && group.FKDevice == deviceId ).Select( detail => detail )
								.ToList() );
			}
			else
			{
				var user = RootRepository.SecurityRepository.GetLoggedInUser();
				if( user.DefaultReachRole == null ||
					( !user.DefaultReachRole.ManageAssignedEvents && user.DefaultReachRole.Name != "Super Admin" ) )
				{
					throw new Exception( "You are not an event manager, please select a device to view events." );
				}

				result =
					Rp.ExecuteAction(
						() => ( from eg in ProxylessContext.EventGroups
							 .Include( gr => gr.EventGroupManagerMaps )
							 .Include( gr => gr.ReaderboardBackgroundImage )
							 .Include( gr => gr.EventGroupManagerMaps.Select( maps => maps.ContactUser ) )

								where eg.IsActive && eg.EventGroupManagerMaps.Any( x => x.FKContactUser == user.PKID )
								select eg ) ).ToList();
			}

			result.ForEach(
				grp =>
				{
					if( !urls.ContainsKey( grp.FKDevice ) )
						urls[ grp.FKDevice ] = GetEventGroupUrlFormat( grp );

					grp.EventUrl = string.Format( urls[ grp.FKDevice ], grp.Guid.ToString( "N" ) );

					if( !hasReader.ContainsKey( grp.FKDevice ) )
						hasReader[ grp.FKDevice ] =
							Context.Devices.First( x => x.PKID == grp.FKDevice )
								.SubDevices.Any( x => x.DeviceType == DeviceType.InfoPointReaderboardTV );
					grp.HasReaderboard = hasReader[ grp.FKDevice ];

					grp.HasReaderboard = hasReader[ grp.FKDevice ];
				}
				);

			return result;
		}

		public EventGroupManagerMap AddEventManager( int groupId, string manager, bool create )
		{
			var eventGroup =
				Rp.ExecuteAction(
					() => ( from eg in Context.EventGroups.Include( x => x.Device.Hotel ) where eg.PKID == groupId select eg ) )
					.FirstOrDefault();
			if( eventGroup == null )
				throw new Exception( "The event group you are trying to modify does not exist." );

			RootRepository.SecurityRepository.AssertEventGroupPemission( eventGroup.PKID );

			var managerCU = Context.ContactUsers.FirstOrDefault( x =>
				x.Email != null && x.Email.ToLower() == manager.ToLower()
				);

			if( managerCU == null && !create )
				return null;

			if( managerCU == null )
			{
				managerCU = new ContactUser
				{
					Email = manager,
					FKAccount = eventGroup.Device.Hotel.FKAccount,
					FKDefaultReachRole = 8,
					ContactUserName = manager,
					ResetPassword = Guid.NewGuid().ToString( "N" ),
					ResetPasswordExpiration = DateTime.UtcNow.AddDays( 3 )
				};
				Context.ContactUsers.Add( managerCU );
			}

			if( eventGroup.EventGroupManagerMaps.Any( x => x.ContactUser.Email == manager ) )
			{
				throw new DataException( "Manager already exists" );
			}

			var map = new EventGroupManagerMap { ContactUser = managerCU };
			eventGroup.EventGroupManagerMaps.Add( map );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return map;
		}

		public void RemoveEventManager( int mapId )
		{
			var map = Context.EventGroupManagerMaps.Include( x => x.EventGroup ).FirstOrDefault( x => x.PKID == mapId );
			if( map == null )
				throw new InvalidDataException( "The event group you are trying to access does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( map.FKEventGroup );

			var user = RootRepository.SecurityRepository.GetLoggedInUser();

			if( user.PKID == map.FKContactUser )
				RootRepository.SecurityRepository.AssertDeviceAuthorization( map.EventGroup.FKDevice );

			Context.EventGroupManagerMaps.Remove( map );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void SaveEvent( EventDetail eventDetail )
		{
			RootRepository.SecurityRepository.AssertEventGroupPemission( eventDetail.FKEventGroup );

			if( !string.IsNullOrEmpty( eventDetail.RecurrenceRule ) )
			{
				var rr = Telerik.Web.UI.RecurrenceRule.TryParse( eventDetail.RecurrenceRule );
				rr.Range.Start = eventDetail.LocalStartDateTime;
				rr.Range.EventDuration = eventDetail.LocalEndDateTime.Subtract( eventDetail.LocalStartDateTime );
				eventDetail.RecurrenceRule = rr.ToString();
			}

			if( eventDetail.PKID == 0 )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( eventDetail.FKDevice );
				var eventGroup = Context.EventGroups.FirstOrDefault( x => x.PKID == eventDetail.FKEventGroup );
				var eventLocation = Context.EventLocations.FirstOrDefault( x => x.PKID == eventDetail.FKEventLocation );
				if( eventGroup != null )
				{
					eventGroup.EventAccessCode = eventDetail.EventGroup.EventAccessCode;
					eventDetail.EventGroup = eventGroup;
				}
				if( eventLocation != null )
				{
					eventDetail.EventLocation = eventLocation;
				}
				if( eventDetail.IsAllDay )
				{
					eventDetail.LocalStartDateTime = eventDetail.LocalStartDateTime.Date;
					eventDetail.LocalEndDateTime = eventDetail.LocalEndDateTime.Date.AddDays( 1 );
				}

				if( eventDetail.FKRecurrenceParentId.HasValue )
				{
					var evt = Context.EventDetails.FirstOrDefault( x => x.PKID == eventDetail.FKRecurrenceParentId.Value );
					if( evt != null )
					{
						evt.RecurrenceRule = RemoveRecurrenceDate( eventDetail.LocalStartDateTime, evt.RecurrenceRule );
					}
				}

				Context.EventDetails.Add( eventDetail );
			}
			else
			{
				var existing = Context.EventDetails.Include( x => x.EventGroup ).FirstOrDefault( x => x.PKID == eventDetail.PKID );
				if( existing == null )
					throw new InvalidDataException( "The event you are trying to access does not exist" );

				var same =
					eventDetail.EventDetailSections.Where( x => existing.EventDetailSections.Any( y => y.PKID == x.PKID ) ).ToList();
				var delete =
					existing.EventDetailSections.Where( x => eventDetail.EventDetailSections.All( y => y.PKID != x.PKID ) ).ToList();
				var add =
					eventDetail.EventDetailSections.Where( x => existing.EventDetailSections.All( y => y.PKID != x.PKID ) ).ToList();

				delete.ForEach( x => Context.EventDetailSections.Remove( x ) );
				add.ForEach( x =>
				{
					x.EventDetailSectionAttachmentMaps.ForEach( att =>
					{
						att.EventDetailSection = x;
						Context.EventDetailSectionAttachmentMaps.Add( att );
					} );
					x.EventDetailSectionImageMaps.ForEach( img =>
					{
						img.EventDetailSection = x;
						Context.EventDetailSectionImageMaps.Add( img );
					} );
					x.EventDetailSectionSponsorshipMaps.ForEach( spn =>
					{
						spn.EventDetailSection = x;
						Context.EventDetailSectionSponsorshipMaps.Add( spn );
					} );
					Context.EventDetailSections.Add( x );
					existing.EventDetailSections.Add( x );
				} );

				foreach( var id in same.Select( x => x.PKID ) )
				{
					var s1 = eventDetail.EventDetailSections.FirstOrDefault( x => x.PKID == id );
					var s2 = existing.EventDetailSections.FirstOrDefault( x => x.PKID == id );

					if( s2 == null || s1 == null )
						continue;

					s2.Name = s1.Name;
					s2.Ordinal = s1.Ordinal;
					s2.Description = s1.Description;
					s2.LocalizedDescription = s1.LocalizedDescription;
					s2.LocalizedName = s1.LocalizedName;
					s2.Website = s1.Website;

					var sameAtt =
						s1.EventDetailSectionAttachmentMaps.Where( x => s2.EventDetailSectionAttachmentMaps.Any( y => y.PKID == x.PKID ) )
							.ToList();
					var deleteAtt =
						s2.EventDetailSectionAttachmentMaps.Where( x => s1.EventDetailSectionAttachmentMaps.All( y => y.PKID != x.PKID ) )
							.ToList();
					var addAtt =
						s1.EventDetailSectionAttachmentMaps.Where( x => s2.EventDetailSectionAttachmentMaps.All( y => y.PKID != x.PKID ) )
							.ToList();

					deleteAtt.ForEach( x => Context.EventDetailSectionAttachmentMaps.Remove( x ) );
					addAtt.ForEach( x =>
					{
						Context.EventDetailSectionAttachmentMaps.Add( x );
						s2.EventDetailSectionAttachmentMaps.Add( x );
					} );
					foreach( var spnId in sameAtt.Select( x => x.PKID ) )
					{
						var spn1 = s1.EventDetailSectionAttachmentMaps.FirstOrDefault( x => x.PKID == spnId );
						var spn2 = s2.EventDetailSectionAttachmentMaps.FirstOrDefault( x => x.PKID == spnId );

						if( spn2 != null && spn1 != null )
							spn2.Ordinal = spn1.Ordinal;
					}

					var sameImg =
						s1.EventDetailSectionImageMaps.Where( x => s2.EventDetailSectionImageMaps.Any( y => y.PKID == x.PKID ) ).ToList();
					var deleteImg =
						s2.EventDetailSectionImageMaps.Where( x => s1.EventDetailSectionImageMaps.All( y => y.PKID != x.PKID ) ).ToList();
					var addImg =
						s1.EventDetailSectionImageMaps.Where( x => s2.EventDetailSectionImageMaps.All( y => y.PKID != x.PKID ) ).ToList();

					deleteImg.ForEach( x => Context.EventDetailSectionImageMaps.Remove( x ) );
					addImg.ForEach( x =>
					{
						Context.EventDetailSectionImageMaps.Add( x );
						s2.EventDetailSectionImageMaps.Add( x );
					} );
					foreach( var spnId in sameImg.Select( x => x.PKID ) )
					{
						var spn1 = s1.EventDetailSectionImageMaps.FirstOrDefault( x => x.PKID == spnId );
						var spn2 = s2.EventDetailSectionImageMaps.FirstOrDefault( x => x.PKID == spnId );

						if( spn2 == null || spn1 == null )
							continue;

						spn2.FKImage = spn1.FKImage;
						spn2.Ordinal = spn1.Ordinal;
					}

					var sameSpn =
						s1.EventDetailSectionSponsorshipMaps.Where( x => s2.EventDetailSectionSponsorshipMaps.Any( y => y.PKID == x.PKID ) )
							.ToList();
					var deleteSpn =
						s2.EventDetailSectionSponsorshipMaps.Where( x => s1.EventDetailSectionSponsorshipMaps.All( y => y.PKID != x.PKID ) )
							.ToList();
					var addSpn =
						s1.EventDetailSectionSponsorshipMaps.Where( x => s2.EventDetailSectionSponsorshipMaps.All( y => y.PKID != x.PKID ) )
							.ToList();

					deleteSpn.ForEach( x => Context.EventDetailSectionSponsorshipMaps.Remove( x ) );
					addSpn.ForEach( x =>
					{
						Context.EventDetailSectionSponsorshipMaps.Add( x );
						s2.EventDetailSectionSponsorshipMaps.Add( x );
					} );
					foreach( var spnId in sameSpn.Select( x => x.PKID ) )
					{
						var spn1 = s1.EventDetailSectionSponsorshipMaps.FirstOrDefault( x => x.PKID == spnId );
						var spn2 = s2.EventDetailSectionSponsorshipMaps.FirstOrDefault( x => x.PKID == spnId );

						if( spn2 == null || spn1 == null )
							continue;

						spn2.FKImage = spn1.FKImage;
						spn2.Ordinal = spn1.Ordinal;
						spn2.Website = spn1.Website;
					}
				}

				if( eventDetail.IsAllDay )
				{
					eventDetail.LocalStartDateTime = eventDetail.LocalStartDateTime.Date;
					eventDetail.LocalEndDateTime = eventDetail.LocalEndDateTime.Date.AddDays( 1 );
				}
				existing.LocalStartDateTime = eventDetail.LocalStartDateTime;
				existing.LocalEndDateTime = eventDetail.LocalEndDateTime;
				existing.Description = eventDetail.Description;
				existing.FKDevice = eventDetail.FKDevice;
				existing.FKEventGroup = eventDetail.FKEventGroup;
				existing.FKEventLocation = eventDetail.FKEventLocation;
				existing.LocalizedDescription = eventDetail.LocalizedDescription;
				existing.LocalizedName = eventDetail.LocalizedName;
				existing.IsActive = eventDetail.IsActive;
				existing.LastModifiedDateTime = DateTime.UtcNow;
				existing.Name = eventDetail.Name;
				existing.RecurrenceRule = eventDetail.RecurrenceRule;
				existing.FKRecurrenceParentId = eventDetail.FKRecurrenceParentId;
				existing.Website = eventDetail.Website;
				existing.EventGroup.EventAccessCode = eventDetail.EventGroup.EventAccessCode;

				if( existing.RecurrenceRule != eventDetail.RecurrenceRule )
				{
					var children = ProxylessContext.EventDetails.Where( x => x.FKRecurrenceParentId == eventDetail.PKID ).ToList();
					children.ForEach( x => ProxylessContext.EventDetails.Remove( x ) );
				}
			}

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public EventDetail GetEvent( int eventId, DateTime? dateToSlice )
		{
			var evt = ProxylessContext.EventDetails
					.Include( x => x.EventGroup )
					.Include( x => x.EventLocation )
					.Include( x => x.EventDetailSections.Select( y => y.EventDetailSectionAttachmentMaps ) )
					.Include( x => x.EventDetailSections.Select( y => y.EventDetailSectionAttachmentMaps.Select( z => z.BlobFile ) ) )
					.Include( x => x.EventDetailSections.Select( y => y.EventDetailSectionImageMaps ) )
					.Include( x => x.EventDetailSections.Select( y => y.EventDetailSectionImageMaps.Select( z => z.Image ) ) )
					.Include( x => x.EventDetailSections.Select( y => y.EventDetailSectionSponsorshipMaps ) )
					.Include( x => x.EventDetailSections.Select( y => y.EventDetailSectionSponsorshipMaps.Select( z => z.Image ) ) )
					.FirstOrDefault( x => x.PKID == eventId && x.IsActive && x.EventGroup.IsActive && x.EventLocation.IsActive );

			if( evt == null )
				throw new InvalidDataException( "The event you are trying to access does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( evt.FKEventGroup );

			evt.IsAllDay = evt.LocalStartDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) &&
						   evt.LocalEndDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) &&
						   evt.LocalStartDateTime < evt.LocalEndDateTime;
			if( evt.IsAllDay )
			{
				evt.LocalStartDateTime = evt.LocalStartDateTime.Date;
				evt.LocalEndDateTime = evt.LocalEndDateTime.Date.AddDays( -1 );
			}

			evt.EventGroup.EventUrl = GetEventGroupUrl( evt.EventGroup );
			if( !dateToSlice.HasValue )
			{
				return evt;
			}
			else
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( evt.FKDevice );

				var utc = dateToSlice.Value.ToUniversalTime();
				evt.RecurrenceRule = null;
				evt.LocalEndDateTime = new DateTime( utc.Year, utc.Month, utc.Day, evt.LocalEndDateTime.Hour,
					evt.LocalEndDateTime.Minute, evt.LocalEndDateTime.Second );
				evt.LocalStartDateTime = new DateTime( utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second );
				evt.FKRecurrenceParentId = evt.PKID;
				evt.PKID = 0;

				return evt;
			}
		}

		private string RemoveRecurrenceDate( DateTime exclusionDate, string recurrenceRule )
		{
			var rules = recurrenceRule.Split( new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
			var exdate = exclusionDate.ToString( "yyyyMMdd" ) + "T" + exclusionDate.ToString( "HHmmss" ) + "Z";
			//EXDATE:20131003T000000Z,20140103T000000Z
			if( rules.Length > 3 )
			{
				var rule = rules[ 3 ];
				var r = rule.Split( ':' );
				if( !r.Contains( exdate ) )
					return recurrenceRule + "," + exdate;

				return recurrenceRule;
			}

			return recurrenceRule + "\r\nEXDATE:" + exdate;
		}

		public EventGroup AddEventGroup( int deviceId, string name )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
			var group = new EventGroup { FKDevice = deviceId, Name = name, IsActive = true };
			ProxylessContext.EventGroups.Add( group );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			group =
				ProxylessContext.EventGroups
					.FirstOrDefault( x => x.PKID == group.PKID );

			if( group != null )
				group.EventUrl = GetEventGroupUrl( group );
			return group;
		}

		public EventLocation AddEventLocation( int deviceId, string name )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
			var location = new EventLocation { FKDevice = deviceId, Name = name, IsActive = true };
			ProxylessContext.EventLocations.Add( location );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			location =
				ProxylessContext.EventLocations
					.FirstOrDefault( x => x.PKID == location.PKID );

			return location;
		}

		public EventGroup RenameEventGroup( int id, string name )
		{
			var group = ProxylessContext.EventGroups.FirstOrDefault( grp => grp.PKID == id );
			if( group == null )
				throw new DataException( "The group you are trying to rename does not exist" );

			RootRepository.SecurityRepository.AssertDevicePermissions( group.FKDevice );
			group.Name = name;
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			return group;
		}

		public void DeleteEventGroup( int id )
		{
			var group = ProxylessContext.EventGroups.FirstOrDefault( grp => grp.PKID == id );
			if( group == null )
				throw new DataException( "The group you are trying to rename does not exist" );
			RootRepository.SecurityRepository.AssertDevicePermissions( group.FKDevice );
			ProxylessContext.EventGroups.Remove( group );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void DeleteEvent( int id, DateTime? dateToSlice )
		{
			if( !dateToSlice.HasValue )
			{
				var evt = ProxylessContext.EventDetails.FirstOrDefault( e => e.PKID == id );
				if( evt == null )
					throw new DataException( "The event you are trying to delete does not exist" );
				RootRepository.SecurityRepository.AssertDevicePermissions( evt.FKDevice );

				var children = ProxylessContext.EventDetails.Where( x => x.FKRecurrenceParentId == id ).ToList();
				children.ForEach( x => ProxylessContext.EventDetails.Remove( x ) );
				ProxylessContext.EventDetails.Remove( evt );
				ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			}
			else
			{
				var evt = ProxylessContext.EventDetails.FirstOrDefault( e => e.PKID == id );
				if( evt == null )
					throw new DataException( "The event you are trying to delete does not exist" );
				RootRepository.SecurityRepository.AssertDevicePermissions( evt.FKDevice );

				evt.RecurrenceRule = RemoveRecurrenceDate( dateToSlice.Value.ToUniversalTime(), evt.RecurrenceRule );

				ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			}
		}

		public object AddEventSection( int eventId )
		{
			var evt =
				ProxylessContext.EventDetails.Include( x => x.EventDetailSections ).FirstOrDefault( grp => grp.PKID == eventId );
			if( evt == null )
				throw new DataException( "The event you are trying to add to does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( evt.FKEventGroup );

			var next = evt.EventDetailSections.Count > 0 ? evt.EventDetailSections.Max( x => x.Ordinal ) + 1 : 1;

			var section = new EventDetailSection { Ordinal = next, FKEventDetail = evt.PKID };
			ProxylessContext.EventDetailSections.Add( section );
			evt.EventDetailSections.Add( section );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return section;
		}

		public void RemoveEventSection( int sectionId )
		{
			var section = Context.EventDetailSections.Include( x => x.EventDetail ).FirstOrDefault( grp => grp.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to delete does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			Context.EventDetailSections.Remove( section );

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public EventDetailSectionImageMap MapEventSectionImage( string path, string name, int width, int height, int sectionId )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var section =
				ProxylessContext.EventDetailSections.Include( x => x.EventDetail )
					.Include( x => x.EventDetailSectionImageMaps )
					.FirstOrDefault( grp => grp.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to modify does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var image = new Image
			{
				FKAccount = user.FKAccount,
				Path = path,
				Width = width,
				Height = height,
				Name = name,
				IsActive = true
			};

			var newOrdinal = section.EventDetailSectionImageMaps.Count > 0
				? section.EventDetailSectionImageMaps.Max( x => x.Ordinal ) + 1
				: 1;

			var map = new EventDetailSectionImageMap
			{
				EventDetailSection = section,
				Image = image,
				FKEventDetailSection = section.PKID,
				Ordinal = newOrdinal
			};

			ProxylessContext.Images.Add( image );
			ProxylessContext.EventDetailSectionImageMaps.Add( map );
			section.EventDetailSectionImageMaps.Add( map );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return map;
		}

		public void RemoveEventSectionImage( int mapId )
		{
			var map =
				Context.EventDetailSectionImageMaps.Include( x => x.EventDetailSection.EventDetail )
					.FirstOrDefault( m => m.PKID == mapId );
			if( map == null )
				throw new DataException( "The image you are trying to remove does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( map.EventDetailSection.EventDetail.FKEventGroup );

			Context.EventDetailSectionImageMaps.Remove( map );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public EventDetailSectionSponsorshipMap AddEventSectionSponsorship( int sectionId )
		{
			var section =
				ProxylessContext.EventDetailSections
					.Include( x => x.EventDetail )
					.Include( x => x.EventDetailSectionSponsorshipMaps )
					.FirstOrDefault( sec => sec.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to add to does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var next = section.EventDetailSectionSponsorshipMaps.Count > 0
				? section.EventDetailSectionSponsorshipMaps.Max( x => x.Ordinal ) + 1
				: 1;

			var sponsorship = new EventDetailSectionSponsorshipMap
			{
				Ordinal = next,
				FKEventDetailSection = section.PKID,
				Website = ""
			};
			ProxylessContext.EventDetailSectionSponsorshipMaps.Add( sponsorship );
			section.EventDetailSectionSponsorshipMaps.Add( sponsorship );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return sponsorship;
		}

		public void RemoveEventSectionSponsorship( int sponsorshipId )
		{
			var map =
				Context.EventDetailSectionSponsorshipMaps.Include( x => x.EventDetailSection.EventDetail )
					.FirstOrDefault( m => m.PKID == sponsorshipId );
			if( map == null )
				throw new DataException( "The sponsorship you are trying to remove does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( map.EventDetailSection.EventDetail.FKEventGroup );

			Context.EventDetailSectionSponsorshipMaps.Remove( map );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void RemoveEventSectionImageGallery( int sectionId )
		{
			var section =
				Context.EventDetailSections
					.Include( x => x.EventDetail )
					.Include( x => x.EventDetailSectionImageMaps )
					.FirstOrDefault( m => m.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to modify does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var results = section.EventDetailSectionImageMaps.ToList();
			results.ForEach( x => Context.EventDetailSectionImageMaps.Remove( x ) );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void RemoveEventSectionSponsorships( int sectionId )
		{
			var section =
				Context.EventDetailSections
					.Include( x => x.EventDetail )
					.Include( x => x.EventDetailSectionSponsorshipMaps )
					.FirstOrDefault( m => m.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to modify does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var results = section.EventDetailSectionSponsorshipMaps.ToList();
			results.ForEach( x => Context.EventDetailSectionSponsorshipMaps.Remove( x ) );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public EventDetailSectionAttachmentMap MapEventSectionAttachment( string path, string contentType, string fileName,
			int sectionId )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var section = ProxylessContext.EventDetailSections
				.Include( x => x.EventDetail )
				.Include( x => x.EventDetailSectionAttachmentMaps )
				.FirstOrDefault( grp => grp.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to modify does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var newOrdinal = section.EventDetailSectionAttachmentMaps.Count > 0
				? section.EventDetailSectionAttachmentMaps.Max( x => x.Ordinal ) + 1
				: 1;

			var attachment = new BlobFile
			{
				FKAccount = user.FKAccount.HasValue ? user.FKAccount.Value : 0,
				ContentType = contentType,
				FileName = fileName,
				Path = path,
				Name = fileName
			};

			var map = new EventDetailSectionAttachmentMap
			{
				EventDetailSection = section,
				FKEventDetailSection = section.PKID,
				Ordinal = newOrdinal,
				BlobFile = attachment
			};

			ProxylessContext.BlobFiles.Add( attachment );
			ProxylessContext.EventDetailSectionAttachmentMaps.Add( map );
			section.EventDetailSectionAttachmentMaps.Add( map );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return map;
		}

		public void RemoveEventSectionAttachment( int mapId )
		{
			var map =
				Context.EventDetailSectionAttachmentMaps
					.Include( x => x.EventDetailSection.EventDetail )
					.FirstOrDefault( m => m.PKID == mapId );
			if( map == null )
				throw new DataException( "The attachment you are trying to remove does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( map.EventDetailSection.EventDetail.FKEventGroup );

			Context.EventDetailSectionAttachmentMaps.Remove( map );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void RemoveEventAttachments( int sectionId )
		{
			var section =
				Context.EventDetailSections
					.Include( x => x.EventDetail )
					.Include( x => x.EventDetailSectionAttachmentMaps )
					.FirstOrDefault( m => m.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to modify does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var results = section.EventDetailSectionAttachmentMaps.ToList();
			results.ForEach( x => Context.EventDetailSectionAttachmentMaps.Remove( x ) );
			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public int ImportEnvisionEvent( int deviceId, string bookingNumber, string eventAccessCode )
		{
			if( string.IsNullOrEmpty( bookingNumber ) )
				throw new ArgumentNullException( "bookingNumber" );
			var device = Context.Devices.FirstOrDefault( x => x.PKID == deviceId );
			if( device == null )
				throw new DataException( "The device you are trying to import an event to does not exist" );
			RootRepository.SecurityRepository.AssertDeviceAuthorization( device.PKID );
			if( !string.IsNullOrEmpty( ( eventAccessCode ) ) )
			{
				var codes =
					Context.EventGroups.Where( g => g.FKDevice == device.PKID && !string.IsNullOrEmpty( g.EventAccessCode ) )
						.ToDictionary( g => g.EventAccessCode );
				eventAccessCode = eventAccessCode.Trim().ToUpper();
				if( codes.ContainsKey( eventAccessCode ) )
					throw new EntitySqlException( "EventGroup.EventAccessCode" );
			}
			var bns =
				Context.HotelEventBookingNumberMaps.FirstOrDefault(
					heb =>
						!string.IsNullOrEmpty( heb.BookingNumber ) && heb.BookingNumber.Trim().ToLower() == bookingNumber.Trim().ToLower() &&
						heb.FKHotel == device.FKHotel );
			if( bns != null )
				throw new EntitySqlException( "HotelEventBookingNumberMaps" );
			var eventDetail = RootRepository.EnvisionRepository.ImportEnvisionEvent( device.PKID, bookingNumber, eventAccessCode );

			return eventDetail.FKEventGroup;
		}

		public EventDetail GetFirstEventOfGroup( int groupId )
		{
			var group = Context.EventGroups.Include( x => x.EventDetails ).FirstOrDefault( x => x.PKID == groupId && x.IsActive );
			if( group == null )
				throw new DataException( "The event group you are trying to get does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( group.PKID );

			if( group.EventDetails == null || group.EventDetails.Any( x => x.IsActive ) )
				throw new DataException( "The event group you are trying to query doesn't have any events." );
			var firstEventTime = group.EventDetails.Where( x => x.IsActive ).Min( x => x.LocalStartDateTime );
			return group.EventDetails.FirstOrDefault( x => x.IsActive && x.LocalStartDateTime == firstEventTime );
		}

		public object MapEventSectionSponsorshipImage( int? mapId, string path, string name, int width, int height,
			int sectionId )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var section =
				ProxylessContext.EventDetailSections
					.Include( x => x.EventDetail )
					.Include( x => x.EventDetailSectionSponsorshipMaps )
					.FirstOrDefault( sec => sec.PKID == sectionId );
			if( section == null )
				throw new DataException( "The section you are trying to add to does not exist" );
			RootRepository.SecurityRepository.AssertEventGroupPemission( section.EventDetail.FKEventGroup );

			var next = section.EventDetailSectionSponsorshipMaps.Count > 0
				? section.EventDetailSectionSponsorshipMaps.Max( x => x.Ordinal ) + 1
				: 1;
			var sponsorship = mapId.HasValue
				? ProxylessContext.EventDetailSectionSponsorshipMaps.FirstOrDefault( x => x.PKID == mapId.Value )
				: null;
			if( sponsorship == null )
			{
				sponsorship = new EventDetailSectionSponsorshipMap
				{
					Ordinal = next,
					FKEventDetailSection = section.PKID,
					Website = ""
				};
				ProxylessContext.EventDetailSectionSponsorshipMaps.Add( sponsorship );
				section.EventDetailSectionSponsorshipMaps.Add( sponsorship );
			}

			var image = new Image
			{
				FKAccount = user.FKAccount,
				Path = path,
				Width = width,
				Height = height,
				Name = name,
				IsActive = true
			};

			ProxylessContext.Images.Add( image );
			sponsorship.Image = image;
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return sponsorship;
		}

		public void SendEventManagerInvite( int mapId )
		{
			var managerMap = Context.EventGroupManagerMaps
				.Include( x => x.ContactUser )
				.Include( x => x.EventGroup.Device.Hotel )
				.FirstOrDefault( x => x.PKID == mapId );
			if( managerMap == null )
				throw new Exception( "The event manager you are trying to send an invite to does not exist" );

			RootRepository.SecurityRepository.AssertEventGroupPemission( managerMap.FKEventGroup );

			RootRepository.EmailRepository.SendEventPlannerInvite( managerMap.ContactUser, false,
				managerMap.EventGroup.Device.Hotel.Name );
		}

		public EventGroup GetEventsFromGuid( MobileApp app, Guid guid )
		{
			if( app == null )
				throw new ArgumentNullException( "app" );
			if( guid == null )
				throw new ArgumentNullException( "guid" );

			var eventGroup = ProxylessContext.EventGroups
				.Include( eg => eg.EventDetails.Select( ed => ed.EventLocation ) )
				.Include( eg => eg.Device.Hotel.HotelDetail.LogoOnWhiteImage )
				.Include( eg => eg.Device.Hotel.HotelBrandMaps.Select( hbm => hbm.HotelBrand.LogoOnWhiteImage ) )
				.Include( eg => eg.Device.Hotel.HotelBrandMaps.Select( hbm => hbm.HotelBrand.MobileAppMaps ) )
				.Include( eg => eg.Device.Hotel.MobileAppMaps )
				.Include( eg => eg.Device.DeviceDetail )
				.FirstOrDefault( x => x.Guid == guid && x.IsActive );

			if( eventGroup == null )
				throw new Exception( "The event group you are trying to get doesn't exist" );

			eventGroup.EventDetails =
				eventGroup.EventDetails.Where( x => x.IsActive && !x.StaffOnly && x.EventLocation.IsActive ).ToList();

			if( app.AppShort.Equals( "connect", StringComparison.InvariantCultureIgnoreCase ) )
				return eventGroup;

			if( eventGroup.Device.Hotel.HotelBrandMaps.Any( x => x.HotelBrand.MobileAppMaps.Any( y => y.FKMobileApp == app.PKID ) )
				||
				eventGroup.Device.Hotel.MobileAppMaps.Any( x => x.FKMobileApp == app.PKID ) )
				return eventGroup;

			throw new Exception( "The event group you are trying to get doesn't belong to the app you supplied" );
		}

		public bool HasSyncer( int deviceId )
		{
			var device =
				Context.Devices
					.Include( d => d.Hotel.HotelDetail )
					.Include( d => d.DeviceDetail )
					.Include(d => d.Hotel.HotelConfigs)
					.FirstOrDefault( d => d.PKID == deviceId );
			if( device == null )
				throw new Exception( "The device you are trying to get doesn't exist" );
			return
				( !string.IsNullOrEmpty( device.DeviceDetail.DelphiUrl ) && device.DeviceDetail.IsDelphiPublic ) ||
				( !string.IsNullOrEmpty( device.Hotel.HotelDetail.EnvisionFacilityId ) && device.DeviceDetail.UseEnvisionForEvents ) ||
				( !string.IsNullOrEmpty( device.Hotel.HotelDetail.DRIPropertyKey ) ) ||
				( !string.IsNullOrEmpty( device.Hotel.HotelDetail.AdHocEventsSyncUri ) ||
				HotelConfigSetting.ISACEventsEnable.GetHotelSetting<bool>(device.Hotel));
		}

		public EventLocation RenameEventLocation( int id, string name )
		{
			var location = ProxylessContext.EventLocations.FirstOrDefault( grp => grp.PKID == id );
			if( location == null )
				throw new DataException( "The location you are trying to rename does not exist" );

			RootRepository.SecurityRepository.AssertDevicePermissions( location.FKDevice );
			location.Name = name;
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			return location;
		}

		public void DeleteEventLocation( int id )
		{
			var location = ProxylessContext.EventLocations.FirstOrDefault( grp => grp.PKID == id );
			if( location == null )
				throw new DataException( "The location you are trying to rename does not exist" );
			RootRepository.SecurityRepository.AssertDevicePermissions( location.FKDevice );
			ProxylessContext.EventLocations.Remove( location );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public string GetEventGroupUrl( EventGroup eventGroup )
		{
			var hotel =
				Rp.ExecuteAction(
					() => ( from d in Context.Devices
								 .Include( d => d.Hotel.MobileAppMaps.Select( mam => mam.MobileApp ) )
								 .Include( d => d.Hotel.HotelBrandMaps )
								 .Include( d => d.Hotel.HotelBrandMaps.Select( hbm => hbm.HotelBrand ) )
								 .Include( d => d.Hotel.HotelBrandMaps.Select( hbm => hbm.HotelBrand.MobileAppMaps.Select( mam => mam.MobileApp ) ) )
								 .Include( d => d.Hotel.HotelSlugs )
							where d.PKID == eventGroup.FKDevice
							select d.Hotel ) )
					.First();

			var primaryBrand = hotel.HotelBrandMaps.FirstOrDefault( x => x.IsPrimary );
			var otherBrands = primaryBrand == null ? hotel.HotelBrandMaps.ToList() : hotel.HotelBrandMaps.Where( x => x.PKID != primaryBrand.PKID ).ToList();

			// NOTE: This section was added to support routing events for Hyatt Regency Scottsdale to the ConnectWeb
			// events page instead of to events.monscierge.com. Please reference CW-61 (https://monscierge.atlassian.net/browse/CW-61).
			// The explicit check for hotel.PKID == 40 will need to be removed at some future date if it's determined
			// that all event links should route to ConnectWeb.
			// ***
			// Phillip Base - 02 JAN 2015
			var hotelSlug = hotel.HotelSlugs.FirstOrDefault();
			if( hotel.PKID == 40 && hotelSlug != null )
			{
				var brandMap = primaryBrand ?? otherBrands.FirstOrDefault();

				var sb = new StringBuilder();
				sb.AppendFormat( "https://{0}.monscierge.com/{1}/events",
					brandMap == null || !brandMap.HotelBrand.HotelBrandSlugs.Any()
						? "connect"
						: brandMap.HotelBrand.HotelBrandSlugs.First().HotelBrandSlugName,
					hotelSlug.HotelSlugName );

				// ConnectWeb only needs the EventGroup Guid if the event is considered private.
				if( !string.IsNullOrWhiteSpace( eventGroup.EventAccessCode ) )
				{
					sb.AppendFormat( "?eg={0}", eventGroup.Guid.ToString( "N" ) );
				}
				return sb.ToString();
			}
			// End CW-61 block
			// ***************

			MobileApp mobileApp = null;
			//FKMobileTokeniOSProduction != null means that we only generate links for production apps
			if( hotel.MobileAppMaps.Any( mam => mam.MobileApp.FKMobileTokeniOSProduction != null ) )
			{
				//database constraint guarantees that a hotel can only be mapped to one mobile app, no ordering necessary
				mobileApp = hotel.MobileAppMaps.First().MobileApp;
			}
			else if( primaryBrand != null && primaryBrand.HotelBrand.MobileAppMaps.Any( mam => mam.MobileApp.FKMobileTokeniOSProduction != null ) )
			{
				//Use the ordinal to select the most relevant hotel brand to use.  For instance, Novotel will be ordinal #1 on the Novotel app but might be #4 on the Accor app.
				//Ordinals across mobile apps aren't guaranteed unique so order by PKID to make it determinstic
				mobileApp = primaryBrand.HotelBrand.MobileAppMaps
					.OrderBy( mam => mam.Ordinal )
					.ThenBy( mam => mam.PKID )
					.First()
					.MobileApp;
			}
			else if( otherBrands.Any( ob => ob.HotelBrand.MobileAppMaps.Any( mam => mam.MobileApp.FKMobileTokeniOSProduction != null ) ) )
			{
				mobileApp = otherBrands
					.SelectMany( b => b.HotelBrand.MobileAppMaps )
					.Where( mam => mam.MobileApp.FKMobileTokeniOSProduction != null )
					.OrderBy( mam => mam.Ordinal )
					.ThenBy( mam => mam.PKID )
					.First()
					.MobileApp;
			}

			if( mobileApp != null )
			{
				return "https://events.monscierge.com/v/" + mobileApp.AppShort + "/?eg=" + eventGroup.Guid.ToString( "N" );
			}
			else
			{
				return "https://events.monscierge.com/v/Connect/?eg=" + eventGroup.Guid.ToString( "N" );
			}
		}

		public string GetEventGroupUrlFormat( EventGroup eventGroup )
		{
			var hotel =
				Rp.ExecuteAction(
					() => ( from d in Context.Devices
								 .Include( d => d.Hotel.MobileAppMaps.Select( mam => mam.MobileApp ) )
								 .Include( d => d.Hotel.HotelBrandMaps )
								 .Include( d => d.Hotel.HotelBrandMaps.Select( hbm => hbm.HotelBrand ) )
								 .Include( d => d.Hotel.HotelBrandMaps.Select( hbm => hbm.HotelBrand.MobileAppMaps.Select( mam => mam.MobileApp ) ) )
								 .Include( d => d.Hotel.HotelSlugs )
							where d.PKID == eventGroup.FKDevice
							select d.Hotel ) )
					.First();

			var primaryBrand = hotel.HotelBrandMaps.FirstOrDefault( x => x.IsPrimary );
			var otherBrands = primaryBrand == null ? hotel.HotelBrandMaps.ToList() : hotel.HotelBrandMaps.Where( x => x.PKID != primaryBrand.PKID ).ToList();

			// NOTE: This section was added to support routing events for Hyatt Regency Scottsdale to the ConnectWeb
			// events page instead of to events.monscierge.com. Please reference CW-61 (https://monscierge.atlassian.net/browse/CW-61).
			// The explicit check for hotel.PKID == 40 will need to be removed at some future date if it's determined
			// that all event links should route to ConnectWeb.
			// ***
			// Phillip Base - 02 JAN 2015
			var hotelSlug = hotel.HotelSlugs.FirstOrDefault();
			if( hotel.PKID == 40 && hotelSlug != null )
			{
				var brandMap = primaryBrand ?? otherBrands.FirstOrDefault();

				var sb = new StringBuilder();
				sb.AppendFormat( "https://{0}.monscierge.com/{1}/events",
					brandMap == null || !brandMap.HotelBrand.HotelBrandSlugs.Any()
						? "connect"
						: brandMap.HotelBrand.HotelBrandSlugs.First().HotelBrandSlugName,
					hotelSlug.HotelSlugName );

				// ConnectWeb only needs the EventGroup Guid if the event is considered private.
				if( !string.IsNullOrWhiteSpace( eventGroup.EventAccessCode ) )
				{
					sb.AppendFormat( "?eg={0}", eventGroup.Guid.ToString( "N" ) );
				}
				return sb.ToString();
			}
			// End CW-61 block
			// ***************

			MobileApp mobileApp = null;
			//FKMobileTokeniOSProduction != null means that we only generate links for production apps
			if( hotel.MobileAppMaps.Any( mam => mam.MobileApp.FKMobileTokeniOSProduction != null ) )
			{
				//database constraint guarantees that a hotel can only be mapped to one mobile app, no ordering necessary
				mobileApp = hotel.MobileAppMaps.First().MobileApp;
			}
			else if( primaryBrand != null && primaryBrand.HotelBrand.MobileAppMaps.Any( mam => mam.MobileApp.FKMobileTokeniOSProduction != null ) )
			{
				//Use the ordinal to select the most relevant hotel brand to use.  For instance, Novotel will be ordinal #1 on the Novotel app but might be #4 on the Accor app.
				//Ordinals across mobile apps aren't guaranteed unique so order by PKID to make it determinstic
				mobileApp = primaryBrand.HotelBrand.MobileAppMaps
					.OrderBy( mam => mam.Ordinal )
					.ThenBy( mam => mam.PKID )
					.First()
					.MobileApp;
			}
			else if( otherBrands.Any( ob => ob.HotelBrand.MobileAppMaps.Any( mam => mam.MobileApp.FKMobileTokeniOSProduction != null ) ) )
			{
				mobileApp = otherBrands
					.SelectMany( b => b.HotelBrand.MobileAppMaps )
					.Where( mam => mam.MobileApp.FKMobileTokeniOSProduction != null )
					.OrderBy( mam => mam.Ordinal )
					.ThenBy( mam => mam.PKID )
					.First()
					.MobileApp;
			}

			if( mobileApp != null )
			{
				return "https://events.monscierge.com/v/" + mobileApp.AppShort + "/?eg={0}";
			}
			else
			{
				return "https://events.monscierge.com/v/Connect/?eg={0}";
			}
		}

		public List<EventGroup> PreviewFileImport( string path, int deviceId )
		{
			var device = ProxylessContext.Devices.FirstOrDefault( x => x.PKID == deviceId );

			if( device == null )
				throw new InvalidDataException( "The device you are trying to import to does not exist" );

			RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );

			return GetEventsFromCsvBlob( path, deviceId );
		}

		public string FileImport( string path, int deviceId )
		{
			var device = ProxylessContext.Devices.FirstOrDefault( x => x.PKID == deviceId );

			if( device == null )
				throw new InvalidDataException( "The device you are trying to import to does not exist" );

			RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );

			var events = GetFlatEventsFromCsvBlob( path, deviceId );

			ProxylessContext.Configuration.AutoDetectChangesEnabled = false;

			var dbEventGroups = ProxylessContext.EventGroups.Where( x => x.FKDevice == deviceId ).ToDictionarySafe( x => x.Name );
			var dbEventLocations = ProxylessContext.EventLocations.Where( x => x.FKDevice == deviceId ).ToDictionarySafe( x => x.Name );
			var dbEventDetails = ProxylessContext.EventDetails.Where( x => x.FKDevice == deviceId && !string.IsNullOrEmpty( x.ExternalEventID ) ).ToDictionarySafe( x => x.ExternalEventID );

			var groupsAdded = 0;
			var eventsAdded = 0;
			var locationsAdded = 0;

			foreach( var evt in events )
			{
				if( !dbEventDetails.ContainsKey( evt.ExternalEventID ) )
				{
					var newEvt = new EventDetail
					{
						Name = evt.EventName,
						ExternalEventID = evt.ExternalEventID,
						FKDevice = deviceId,
						LocalStartDateTime = evt.Start,
						LocalEndDateTime = evt.End
					};

					if( dbEventGroups.ContainsKey( evt.GroupName ) )
					{
						newEvt.EventGroup = dbEventGroups[ evt.GroupName ];
					}
					else
					{
						var newGroup = new EventGroup
						{
							Name = evt.GroupName,
							IsActive = true,
							FKDevice = deviceId
						};
						newEvt.EventGroup = newGroup;
						ProxylessContext.EventGroups.Add( newGroup );
						dbEventGroups.Add( newGroup.Name, newGroup );

						groupsAdded++;
					}

					if( dbEventLocations.ContainsKey( evt.LocationName ) )
					{
						newEvt.EventLocation = dbEventLocations[ evt.LocationName ];
					}
					else
					{
						var newLocation = new EventLocation
						{
							Name = evt.LocationName,
							IsActive = true,
							FKDevice = deviceId
						};

						newEvt.EventLocation = newLocation;
						ProxylessContext.EventLocations.Add( newLocation );
						dbEventLocations.Add( newLocation.Name, newLocation );

						locationsAdded++;
					}

					ProxylessContext.EventDetails.Add( newEvt );
					dbEventDetails.Add( newEvt.ExternalEventID, newEvt );

					eventsAdded++;
				}
			}

			ProxylessContext.ObjectContext.DetectChanges();
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return string.Format( "The event imported has completed. {0} Event Groups, {1} EventLocations, and {2} EventDetails were added", groupsAdded, locationsAdded, eventsAdded );
		}

		private List<EventGroup> GetEventsFromCsvBlob( string path, int deviceId )
		{
			var eventGroups = new List<EventGroup>();
			var eventLocations = new List<EventLocation>();
			var bytes = MonsciergeServiceUtilities.Utilities.ReadAzureBlob( path );

			using( var reader = new CsvReader( new MemoryStream( bytes ) ) )
			{
				while( reader.HasMoreRecords )
				{
					var record = reader.ReadDataRecord();
					if( record[ 0 ] == "Definite" )
					{
						var eventGroup = eventGroups.FirstOrDefault( x => x.Name == record[ 1 ] );
						if( eventGroup == null )
						{
							eventGroup = new EventGroup
							{
								Name = record[ 1 ],
								FKDevice = deviceId,
								IsActive = true,
								Guid = Guid.NewGuid()
							};
							eventGroups.Add( eventGroup );
						}

						var eventLocation = eventLocations.FirstOrDefault( x => x.Name == record[ 5 ] );
						if( eventLocation == null )
						{
							eventLocation = new EventLocation
							{
								Name = record[ 5 ],
								FKDevice = deviceId,
								IsActive = true
							};
						}
						eventGroup.EventDetails.Add( new EventDetail
						{
							FKDevice = deviceId,
							LocalStartDateTime = DateTime.Parse( record[ 2 ] + " " + record[ 3 ] ),
							LocalEndDateTime = DateTime.Parse( record[ 2 ] + " " + record[ 4 ] ),
							EventLocation = eventLocation,
							ExternalEventID = record[ 6 ],
							Name = record[ 7 ]
						} );
					}
				}
			}

			return eventGroups;
		}

		private List<FlatEvent> GetFlatEventsFromCsvBlob( string path, int deviceId )
		{
			var bytes = MonsciergeServiceUtilities.Utilities.ReadAzureBlob( path );

			var list = new List<FlatEvent>();

			using( var reader = new CsvReader( new MemoryStream( bytes ) ) )
			{
				while( reader.HasMoreRecords )
				{
					var record = reader.ReadDataRecord();
					if( record[ 0 ] == "Definite" || record[ 0 ] == "Confirmed" )
					{
						var flatEvent = new FlatEvent
						{
							GroupName = record[ 1 ],
							LocationName = record[ 5 ],
							Start = DateTime.Parse( record[ 2 ] + " " + record[ 3 ] ),
							End = DateTime.Parse( record[ 2 ] + " " + record[ 4 ] ),
							ExternalEventID = record[ 6 ],
							EventName = record[ 7 ]
						};
						list.Add( flatEvent );
					}
				}
			}

			return list;
		}

		internal class FlatEvent
		{
			public string GroupName { get; set; }

			public string LocationName { get; set; }

			public string EventName { get; set; }

			public string ExternalEventID { get; set; }

			public DateTime Start { get; set; }

			public DateTime End { get; set; }
		}

		public bool HasEnvision( int deviceId )
		{
			var device =
				Context.Devices
					.Include( d => d.Hotel.HotelDetail )
					.Include( d => d.DeviceDetail )
					.Include( d => d.Hotel.HotelConfigs )
					.FirstOrDefault( d => d.PKID == deviceId );
			if( device == null )
				throw new Exception( "The device you are trying to get doesn't exist" );
			return
				(!string.IsNullOrEmpty(device.Hotel.HotelDetail.EnvisionFacilityId) && device.DeviceDetail.UseEnvisionForEvents);
		}
	}
}