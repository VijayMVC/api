using AutoMapper;
using ConnectCMS.Infrastructure.SignalR;
using ConnectCMS.Models;
using ConnectCMS.Models.ContactUser;
using ConnectCMS.Repositories;
using ConnectCMS.Resources;
using ConnectCMS.Utils;
using ConnectCMS.ViewModels.Request;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using MonsciergeWebUtilities.Actions;
using MonsciergeWebUtilities.Utilities;
using Newtonsoft.Json;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	[Authorize]
	public class RequestController : ControllerBase
	{
		private readonly IRequestRepository _requestRepository;
		private readonly DeviceRepository _deviceRepository;

		public RequestController()
		{
			_requestRepository = ConnectCmsRepository.RequestRepository;
			_deviceRepository = ConnectCmsRepository.DeviceRepository;
		}

		// GET: Request
		public ActionResult Index()
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( user == null )
				return RedirectToAction( "Login", "Account" );

			if( !ConnectCmsRepository.SecurityRepository.IsRequestUser() )
				return RedirectToAction( "Index", "Home" );

			ViewBag.RequestUserId = user.RequestUser == null ? 0 : user.RequestUser.PKID;

			//FormsAuthentication.SetAuthCookie( "CU|" + user.PKID, false );

			var appSettings = new AppSettings();
			ViewBag.ShowDevSideBar = !Request.IsAjaxRequest() && appSettings.ShowDevSidebarForNonAjaxRequests;

			ViewBag.CanDelete = ConnectCmsRepository.SecurityRepository.IsSuperAdmin();

			if( !Request.IsAjaxRequest() )
			{
				if( user.FKRequestUser == null )
					return RedirectToLocal( "" );

				if( user.DefaultReachRole.Name == "Super Admin" )
				{
					// If the user is in the Super Admin role, use the pre-configured demo device Id.
					var deviceId = appSettings.RequestHandlingDemoDeviceId;
					ViewBag.DemoDeviceId = ( deviceId.HasValue && deviceId.Value > 0 ) ? string.Format("[{0}]", deviceId.Value) : "[0]";
				}
				else
				{
					ViewBag.HidePlaceHolder = true;
					var userDevices = _deviceRepository.GetDevicesForRequestUser(user.RequestUser.PKID);
					//var userDevices = _deviceRepository.GetDevicesForUser( user.PKID );
					if( userDevices.Any() )
					{
						var device = userDevices.FirstOrDefault( x => x.Hotel.HotelDetail.HasConnect );
						if( device == null )
							return RedirectToLocal( "" );

						ViewBag.DemoDeviceId = string.Format("[{0}]", string.Join(",", userDevices.Select(d => d.PKID)));
					}
					else
					{
						return RedirectToLocal( "" );
					}
				}
			}

			return View( "Index" );
		}

		public ActionResult SMSBlast( int? deviceId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			//FormsAuthentication.SetAuthCookie( "CU|" + user.PKID, false );
			if( !deviceId.HasValue )
			{
				var device = ConnectCmsRepository.DeviceRepository.GetDeviceForLoggedInUser();

				if( device != null )
				{
					ViewBag.DeviceId = device.PKID;
					ViewBag.HotelId = device.FKHotel;
					ViewBag.WelcomeMessage = device.Hotel.HotelDetail.SMSWelcomeMessage;
					ViewBag.SMSStockMessages = ConnectCmsRepository.SmsRepository.GetSMSStockMessages( user.PKID, device.FKHotel ).Select( m => new { m.PKID, m.Name, m.Text, m.FKHotel } ).ToList().ToJSON();
				}
			}
			else
			{
				ConnectCmsRepository.SecurityRepository.AssertDeviceAuthorization( deviceId.Value );
				var device = ConnectCmsRepository.DeviceRepository.GetDevice( deviceId.Value );
				ViewBag.DeviceId = device.PKID;
				ViewBag.HotelId = device.FKHotel;
				ViewBag.WelcomeMessage = device.Hotel.HotelDetail.SMSWelcomeMessage;
				ViewBag.SMSStockMessages = ConnectCmsRepository.SmsRepository.GetSMSStockMessages( user.PKID, device.FKHotel ).Select( m => new { m.PKID, m.Name, Message = m.Text, m.FKHotel } ).ToList().ToJSON();
			}

			return View();
		}

		public ActionResult SMSMessaging( int? deviceId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( !deviceId.HasValue )
			{
				var device = ConnectCmsRepository.DeviceRepository.GetDeviceForLoggedInUser();

				if( device != null )
				{
					ViewBag.DeviceId = device.PKID;
					ViewBag.HotelId = device.FKHotel;
					ViewBag.WelcomeMessage = device.Hotel.HotelDetail.SMSWelcomeMessage;
					if( device.Hotel.HotelDetail.SMSWelcomeRequestType == null ||
						!device.Hotel.HotelDetail.SMSWelcomeRequestType.SMSNumbers.Any() )
					{
						ViewBag.IsConfigured = false;
					}
					else
					{
						ViewBag.IsConfigured = true;
						ViewBag.SMSNumberCount = device.Hotel.HotelDetail.SMSWelcomeRequestType.SMSNumbers.Count();
					}

					ViewBag.SMSStockMessages =
						ConnectCmsRepository.SmsRepository.GetSMSStockMessages( user.PKID, device.FKHotel )
							.Select( m => new { m.PKID, m.Name, m.Text, m.FKHotel } )
							.ToList()
							.ToJSON();
				}
			}
			else
			{
				ConnectCmsRepository.SecurityRepository.AssertDeviceAuthorization( deviceId.Value );
				var device = ConnectCmsRepository.DeviceRepository.GetDevice( deviceId.Value );
				ViewBag.HotelId = device.FKHotel;
				ViewBag.WelcomeMessage = device.Hotel.HotelDetail.SMSWelcomeMessage;
				if( device.Hotel.HotelDetail.SMSWelcomeRequestType == null ||
						!device.Hotel.HotelDetail.SMSWelcomeRequestType.SMSNumbers.Any() )
				{
					ViewBag.IsConfigured = false;
				}
				else
				{
					ViewBag.IsConfigured = true;
					ViewBag.SMSNumberCount = device.Hotel.HotelDetail.SMSWelcomeRequestType.SMSNumbers.Count();
				}
				ViewBag.SMSStockMessages = ConnectCmsRepository.SmsRepository.GetSMSStockMessages( user.PKID, device.FKHotel ).Select( m => new { m.PKID, m.Name, Message = m.Text, m.FKHotel } ).ToList().ToJSON();
			}

			return View();
		}

		public JsonNetResult PreviewSMSBlast( int deviceId, string path )
		{
			var results = ConnectCmsRepository.RequestRepository.PreviewSMSBlast( path, deviceId );
			return JsonNet( results );
		}

		public JsonNetResult UploadSMSBlast( int hotelId, string path )
		{
			var deviceId = ConnectCmsRepository.HotelRepository.GetHotelQuery( hotelId ).Include( x => x.Devices ).SelectMany( x => x.Devices ).First().PKID;
			var results = ConnectCmsRepository.RequestRepository.PreviewSMSBlast( path, deviceId ).Select( x => new { x.FirstName, x.LastName, x.ToPhone } );
			return JsonNet( results );
		}

		public JsonNetResult GetSMSMessageStatus( string sid, string accountSid )
		{
			var results = ConnectCmsRepository.RequestRepository.GetSMSMessageStatus( sid, accountSid );
			return JsonNet( results, JsonRequestBehavior.AllowGet );
		}

		public ActionResult GetRequestTypes( int deviceId )
		{
			var device = _deviceRepository.GetDevice( deviceId );
			if( device == null )
				return new HttpNotFoundResult();

			var requestTypes = _requestRepository.GetRequestTypes( device.FKHotel );

			var viewModel = Mapper.Map<List<RequestTypeViewModel>>( requestTypes );

			var result = new JsonNetResult { Formatting = Formatting.Indented, Data = viewModel };
			return result;
		}

		public ActionResult GetRequestTypesByCategory( int deviceId )
		{
			var device = _deviceRepository.GetDevice( deviceId );
			if( device == null )
				return new HttpNotFoundResult();

			var requestTypes = _requestRepository.GetRequestTypes( device.FKHotel );

			var viewModel =
				requestTypes.GroupBy( x => x.RequestCategory )
					.Select(
						x =>
							new
							{
								RequestCategory =
									new
									{
										Name = x.Key != null ? x.Key.Name : ConnectCMSResources.Uncategorized,
										Ordinal = x.Key != null ? x.Key.Ordinal : -1,
										RequestTypes = Mapper.Map<List<RequestTypeViewModel>>( x.Select( y => y ).OrderBy( y => y.Ordinal ).ToList() )
									}
							} )
					.OrderBy( x => x.RequestCategory.Ordinal );

			var result = new JsonNetResult
			{
				Formatting = Formatting.Indented,
				Data = viewModel,
				SerializerSettings =
					new JsonSerializerSettings
					{
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
						PreserveReferencesHandling = PreserveReferencesHandling.None
					}
			};
			return result;
		}

		public JsonNetResult GetRequestUserSettings()
		{
			var settings = ConnectCmsRepository.SecurityRepository.GetContactUserSettings();
			var dict = settings.ToDictionary( x => ( ( ContactUserSettingKeys )x.Key ).ToString(), x => x.Value );
			var managerGroups = ConnectCmsRepository.RequestRepository.GetRequestManagerGroups();
			dict.Add( "RequestManagerGroups",
				managerGroups.Aggregate( String.Empty,
					( current, s ) => current + ( s.PKID.ToString( CultureInfo.InvariantCulture ).Trim() + "," ) ).TrimEnd( ",".ToCharArray() ) );

			return JsonNet( dict, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult PreSend()
		{
			return JsonNet( true, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult GetSMSJobs( int hotelId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.HotelRepository.CheckHotelPermission( user.PKID, hotelId ).Result != PermissionResults.Authorized )
				throw new SecurityException( "You do not have access to this hotel" );

			var jobs = ConnectCmsRepository.SmsRepository.GetSMSJobs( hotelId ).Select( j => new
			{
				j.PKID,
				j.Name,
				j.FKHotel,
				j.Status,
				j.FKCreatedByContactUser,
				j.CreatedOn,
				j.NotifyEmail,
				CreatorByContactUser = j.FKCreatedByContactUser != null ? new
				{
					j.CreatorByContactUser.PKID,
					j.CreatorByContactUser.Email,
					j.CreatorByContactUser.ContactUserName
				} : null,
				SMSTasks = j.SMSTasks.OrderBy( t => t.RunAt ).Select( t => new
				{
					t.PKID,
					t.FKSMSJob,
					t.Status,
					t.StatusLastUpdated,
					t.RunAt,
					t.FirstName,
					t.LastName,
					t.ToPhone,
					t.Message,
					t.StatusMessage,
					t.MessageId,
					t.FromPhone
				} )
			} ).OrderByDescending( j => j.CreatedOn ).Take( 25 ).ToList();
			//CMSHub.Instance.Subscribe( jobs.Select( x => x.PKID ).ToList() );
			return JsonNet( jobs, JsonRequestBehavior.AllowGet );
		}

		[HttpPost]
		public JsonNetResult CreateSMSJob( SMSJob job )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.HotelRepository.CheckHotelPermission( user.PKID, job.FKHotel ).Result != PermissionResults.Authorized )
				return JsonNet( false, JsonRequestBehavior.AllowGet );
			job.FKCreatedByContactUser = user.PKID;
			var newJob = ConnectCmsRepository.SmsRepository.SaveSMSJob( job, string.Format( "CU|{0}", user.PKID ) );
			return JsonNet( newJob, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult CancelSMSJob( int jobId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.SmsRepository.CheckSMSJobPermission( user.PKID, jobId ).Result != PermissionResults.Authorized )
				throw new SecurityException( "You do not have access to this SMS Job" );
			var job = ConnectCmsRepository.SmsRepository.CancelSMSJob( user.PKID, jobId );

			foreach( var task in job.SMSTasks )
			{
				CMSHub.NotifyJobChange( jobId, task.PKID, ( int )task.SMSJob.Status, ( int )task.Status, task.StatusMessage, task.StatusLastUpdated ?? DateTime.UtcNow );
				SmsJobUtilities.SmsTaskStatusChanged( task, string.Format( "CU|{0}", user.PKID ) );
			}

			return JsonNet( job, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult CancelSMSTask( int taskId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.SmsRepository.CheckSMSTaskPermission( user.PKID, taskId ).Result != PermissionResults.Authorized )
				throw new SecurityException( "You do not have access to this SMS Task" );
			var task = ConnectCmsRepository.SmsRepository.CancelSMSTask( user.PKID, taskId );
			CMSHub.NotifyJobChange( task.FKSMSJob, taskId, ( int )task.SMSJob.Status, ( int )task.Status, task.StatusMessage, task.StatusLastUpdated.Value );
			SmsJobUtilities.SmsTaskStatusChanged( task, string.Format( "CU|{0}", user.PKID ) );
			return JsonNet( task, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult RetrySMSTask( int taskId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.SmsRepository.CheckSMSTaskPermission( user.PKID, taskId ).Result != PermissionResults.Authorized )
				throw new SecurityException( "You do not have access to this SMS Task" );

			var task = ConnectCmsRepository.SmsRepository.RetrySMSTask( user.PKID, taskId );
			CMSHub.NotifyJobChange( task.FKSMSJob, taskId, ( int )task.SMSJob.Status, ( int )task.Status, task.StatusMessage, task.StatusLastUpdated.Value );
			return JsonNet( task, JsonRequestBehavior.AllowGet );
		}

		[HttpPost]
		public JsonNetResult KeepAlive()
		{
			return JsonNet( new { keepAlive = true } );
		}

		[HttpPost]
		public JsonNetResult SaveStockMessage( SMSStockMessage message )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			message = ConnectCmsRepository.SmsRepository.SaveSMSStockMessage( user.PKID, message );
			return JsonNet( message );
		}

		[HttpPost]
		public JsonNetResult DeleteStockMessage( SMSStockMessage message )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ConnectCmsRepository.SmsRepository.DeleteSMSStockMessage( user.PKID, message.PKID );
			return JsonNet( true );
		}

		[HttpPost]
		[AllowAnonymous]
		public JsonNetResult NotifyTaskChanged( int SMSJobId, int SMSTaskId, int SMSJobStatus, int SMSTaskStatus, string SMSTaskStatusMessage, DateTime? SMSTaskLastUpdated )
		{
			CMSHub.NotifyJobChange( SMSJobId, SMSTaskId, SMSJobStatus, SMSTaskStatus, SMSTaskStatusMessage, SMSTaskLastUpdated.Value );
			return JsonNet( true );
		}
	}
}