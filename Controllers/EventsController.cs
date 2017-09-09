using ConnectCMS.Models.ContactUser;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using Newtonsoft.Json;
using PostSharp.Extensibility;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class EventsController : ControllerBase
	{
		#region views

		public ActionResult Index( int? deviceId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( !deviceId.HasValue )
			{
				var device = user.FKDefaultReachRole == 8 ? null : ConnectCmsRepository.DeviceRepository.GetDeviceForLoggedInUser();

				if( device != null )
				{
					ViewBag.DeviceId = device.PKID;
					ViewBag.HasSyncer = ConnectCmsRepository.EventRepository.HasSyncer(device.PKID);
					ViewBag.HasEnvision = ConnectCmsRepository.EventRepository.HasEnvision(device.PKID);
				}
				else
				{
					ViewBag.HasSyncer = false;
				}
			}
			else
			{
				ViewBag.HasSyncer = Request.IsAjaxRequest() && ConnectCmsRepository.EventRepository.HasSyncer( deviceId.Value );
				ViewBag.HasEnvision = Request.IsAjaxRequest() && ConnectCmsRepository.EventRepository.HasEnvision( deviceId.Value );
			}
			ViewBag.DefaultViewIndex =
				user.ContactUserSettings.Where( x => x.Key == ( int )ContactUserSettingKeys.EventsDefaultView )
					.Select( x => x.Value )
					.FirstOrDefault() ?? "0";
			if( string.IsNullOrEmpty( ViewBag.DefaultViewIndex ) )
				ViewBag.DefaultViewIndex = "0";
			ViewBag.IsContentManager = Request.IsAjaxRequest() || ( user.DefaultReachRole.Name != "Super Admin" && !user.DefaultReachRole.ManageAssignedEvents );

			return View( user );
		}

		public ActionResult EventsImporter( int deviceId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();

			ConnectCmsRepository.SecurityRepository.AssertSuperAdmin();

			ViewBag.CanImport = !ConnectCmsRepository.EventRepository.HasSyncer( deviceId );

			return View( user );
		}

		public ActionResult ImportEnvision()
		{
			return PartialView( "_ImportEnvision" );
		}

		public ActionResult Recurrence()
		{
			return PartialView( "_Recurrence" );
		}

		public ActionResult EditRecurrenceDialog()
		{
			return PartialView( "_EditRecurrenceDialog" );
		}

		public ActionResult DeleteRecurrenceDialog()
		{
			return PartialView( "_DeleteRecurrenceDialog" );
		}

		#endregion views

		#region EventGroup

		[HttpPost]
		public JsonNetResult AddEventGroup( int deviceId, string name )
		{
			var group = ConnectCmsRepository.EventRepository.AddEventGroup( deviceId, name );
			return JsonNet( group );
		}

		[HttpPost]
		public JsonNetResult AddEventLocation( int deviceId, string name )
		{
			var location = ConnectCmsRepository.EventRepository.AddEventLocation( deviceId, name );
			return JsonNet( location );
		}

		[HttpPost]
		public JsonNetResult RenameEventGroup( int id, string name )
		{
			var group = ConnectCmsRepository.EventRepository.RenameEventGroup( id, name );
			return JsonNet( group );
		}

		[HttpPost]
		public JsonNetResult RenameEventLocation( int id, string name )
		{
			var location = ConnectCmsRepository.EventRepository.RenameEventLocation( id, name );
			return JsonNet( location );
		}

		[HttpPost]
		public JsonNetResult DeleteEventGroup( int id )
		{
			ConnectCmsRepository.EventRepository.DeleteEventGroup( id );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult DeleteEventLocation( int id )
		{
			ConnectCmsRepository.EventRepository.DeleteEventLocation( id );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult AddEventManager( int groupId, string manager, bool? create )
		{
			var map = ConnectCmsRepository.EventRepository.AddEventManager( groupId, manager, create.HasValue && create.Value );

			if( map != null )
				ConnectCmsRepository.EmailRepository.SendEventPlannerInvite( map.ContactUser, create, map.EventGroup.Device.Hotel.Name );

			return JsonNet(
				map != null ? new { MapId = map.PKID, UserId = map.FKContactUser } : new { MapId = 0, UserId = 0 }
				);
		}

		[HttpPost]
		public JsonNetResult ResendInvite( int mapId )
		{
			ConnectCmsRepository.EventRepository.SendEventManagerInvite( mapId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult RemoveEventManager( int mapId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventManager( mapId );
			return JsonNet( true );
		}

		#endregion EventGroup

		#region EventSection

		[HttpPost]
		public JsonNetResult AddEventSection( int eventId )
		{
			var section = ConnectCmsRepository.EventRepository.AddEventSection( eventId );
			return JsonNet( section );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSection( int sectionId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventSection( sectionId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult MapEventSectionImage( string path, string name, int width, int height, int sectionId )
		{
			var imageMap = ConnectCmsRepository.EventRepository.MapEventSectionImage( path, name, width, height, sectionId );
			return JsonNet( imageMap );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSectionImage( int mapId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventSectionImage( mapId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSectionImageGallery( int sectionId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventSectionImageGallery( sectionId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult AddEventSectionSponsorship( int sectionId )
		{
			var sponsorship = ConnectCmsRepository.EventRepository.AddEventSectionSponsorship( sectionId );
			return JsonNet( sponsorship );
		}

		[HttpPost]
		public JsonNetResult MapEventSectionSponsorshipImage( int? mapId, string path, string name, int width, int height, int sectionId )
		{
			var imageMap = ConnectCmsRepository.EventRepository.MapEventSectionSponsorshipImage( mapId, path, name, width, height, sectionId );
			return JsonNet( imageMap );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSectionSponsorship( int sponsorshipId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventSectionSponsorship( sponsorshipId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSectionSponsorships( int sectionId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventSectionSponsorships( sectionId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult MapEventSectionAttachment( string path, string contentType, string fileName, int sectionId )
		{
			var attachment = ConnectCmsRepository.EventRepository.MapEventSectionAttachment( path, contentType, fileName, sectionId );
			return JsonNet( attachment );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSectionAttachment( int mapId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventSectionAttachment( mapId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult RemoveEventSectionAttachments( int sectionId )
		{
			ConnectCmsRepository.EventRepository.RemoveEventAttachments( sectionId );
			return JsonNet( true );
		}

		#endregion EventSection

		#region Events

		[HttpPost]
		public JsonNetResult GetEvents( int? deviceId, string view, DateTime date )
		{
			var events = ConnectCmsRepository.EventRepository.GetEvents( deviceId, view, date );
			return
				JsonNet(
					events.Select(
						x =>
							new EventDetail
							{
								EventGroup = new EventGroup { Name = x.EventGroup.Name, PKID = x.EventGroup.PKID, EventAccessCode = x.EventGroup.EventAccessCode },
								EventLocation = new EventLocation { Name = x.EventLocation.Name, PKID = x.EventLocation.PKID },
								Name = x.Name,
								LocalStartDateTime = x.LocalStartDateTime,
								LocalEndDateTime = x.LocalEndDateTime,
								IsAllDay = x.IsAllDay,
								RecurrenceRule = string.IsNullOrEmpty( x.RecurrenceRule ) ? null : x.RecurrenceRule.TrimEnd(),
								FKRecurrenceParentId = x.FKRecurrenceParentId,
								IsActive = x.IsActive,
								PKID = x.PKID,
								Occurrences = x.Occurrences,
								StaffOnly = x.StaffOnly
							} ) );
		}

		[HttpPost]
		public JsonNetResult GetGroupsAndLocations( int? deviceId )
		{
			var groups = ConnectCmsRepository.EventRepository.GetEventGroups( deviceId );
			var locations = ConnectCmsRepository.EventRepository.GetEventLocations( deviceId );
			groups.ForEach(
				x => x.EventGroupManagerMaps.ForEach(
					y =>
						y.ContactUser = new ContactUser
						{
							Email = y.ContactUser.Email
						}
					) );
			return JsonNet( new { Groups = groups, Locations = locations } );
		}

		[HttpPost]
		public JsonNetResult UpdateEventTime( EventDetail eventDetail )
		{
			eventDetail.LocalStartDateTime = eventDetail.LocalStartDateTime.ToUniversalTime();
			eventDetail.LocalEndDateTime = eventDetail.LocalEndDateTime.ToUniversalTime();
			ConnectCmsRepository.EventRepository.UpdateTime( eventDetail );
			return JsonNet( eventDetail );
		}

		[HttpPost]
		public JsonNetResult SaveEvent( EventDetail eventDetail )
		{
			eventDetail.LocalStartDateTime = eventDetail.LocalStartDateTime.ToUniversalTime();
			eventDetail.LocalEndDateTime = eventDetail.LocalEndDateTime.ToUniversalTime();
			ConnectCmsRepository.EventRepository.SaveEvent( eventDetail );
			return JsonNet( eventDetail.PKID );
		}

		[HttpPost]
		public JsonNetResult DeleteEvent( int id, DateTime? dateToSlice )
		{
			ConnectCmsRepository.EventRepository.DeleteEvent( id, dateToSlice );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult GetEvent( int eventId, DateTime? dateToSlice )
		{
			var evt = ConnectCmsRepository.EventRepository.GetEvent( eventId, dateToSlice );
			return JsonNet( evt );
		}

		[HttpPost]
		public JsonNetResult ImportEnvisionEvent( int deviceId, string bookingNumber, string eventAccessCode )
		{
			try
			{
				var groupId = ConnectCmsRepository.EventRepository.ImportEnvisionEvent( deviceId, bookingNumber, eventAccessCode );
				var firstEvent = ConnectCmsRepository.EventRepository.GetFirstEventOfGroup( groupId );
				return JsonNet( new { error = "", groupId, firstEventDateTime = firstEvent.LocalStartDateTime } );
			}
			catch( EntitySqlException e )
			{
				return JsonNet( new { error = e.Message } );
			}
		}

		#endregion Events

		public JsonNetResult PreviewImportFile( string path, int deviceId )
		{
			var eventGroups = ConnectCmsRepository.EventRepository.PreviewFileImport( path, deviceId );
			return JsonNet( eventGroups );
		}

		public JsonNetResult ImportFile( string path, int deviceId )
		{
			var result = ConnectCmsRepository.EventRepository.FileImport( path, deviceId );
			return JsonNet( result );
		}
	}
}