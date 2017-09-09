using System;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Web.Mvc;
using ConnectCMS.Infrastructure.SignalR;
using ConnectCMS.Models;
using ConnectCMS.Utils;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	[Authorize]
	public class NotificationController : ControllerBase
	{
		public ActionResult Jobs( int? deviceId )
		{
			if( !deviceId.HasValue )
			{
				var device = ConnectCmsRepository.DeviceRepository.GetDeviceForLoggedInUser();

				if( device != null )
				{
					ViewBag.DeviceId = device.PKID;
					ViewBag.HotelId = device.FKHotel;

					var brand = device.Hotel.HotelBrandMaps.OrderBy( x => x.IsPrimary ? 0 : 1 ).Select( x => x.HotelBrand ).FirstOrDefault();
					if( brand != null )
					{
						ViewBag.BannerColor = brand.ThemeColor1.HasValue ? ColorTranslator.ToHtml( MediaUtilities.ColorFromInt( brand.ThemeColor1.Value ).Value ) : null;
						ViewBag.TextColor = brand.ThemeColor2.HasValue ? ColorTranslator.ToHtml( MediaUtilities.ColorFromInt( brand.ThemeColor2.Value ).Value ) : null;
					}
				}
			}
			else
			{
				ConnectCmsRepository.SecurityRepository.AssertDeviceAuthorization( deviceId.Value );
				var device = ConnectCmsRepository.DeviceRepository.GetDevice( deviceId.Value );
				ViewBag.HotelId = device.FKHotel;

				var brand = device.Hotel.HotelBrandMaps.OrderBy(x => x.IsPrimary ? 0 : 1).Select(x => x.HotelBrand).FirstOrDefault();
				if (brand != null)
				{
					ViewBag.BannerColor = brand.ThemeColor1.HasValue ? ColorTranslator.ToHtml(MediaUtilities.ColorFromInt( brand.ThemeColor1.Value ).Value) : null;
					ViewBag.TextColor =  brand.ThemeColor2.HasValue ? ColorTranslator.ToHtml(MediaUtilities.ColorFromInt(brand.ThemeColor2.Value).Value) : null;
				}

			}

			return View();
		}

		public JsonNetResult PreSend()
		{
			return JsonNet( true, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult GetNotificationJobs( int hotelId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.HotelRepository.CheckHotelPermission( user.PKID, hotelId ).Result != PermissionResults.Authorized )
				throw new SecurityException( "You do not have access to this hotel" );

			var jobs = ConnectCmsRepository.NotificationRepository.GetNotificationJobs( hotelId ).Select( j => new
			{
				j.PKID,
				j.Name,
				j.FKHotel,
				j.Status,
				j.FKCreatedByContactUser,
				j.CreatedOn,
				j.UpdatedOn,
				j.StartDateTime,
				CreatorByContactUser = new
				{
					j.ContactUser.PKID,
					j.ContactUser.Email,
					j.ContactUser.ContactUserName
				}
			} ).OrderByDescending( j => j.CreatedOn ).Take( 25 ).ToList();
			//CMSHub.Instance.Subscribe( jobs.Select( x => x.PKID ).ToList() );
			return JsonNet( jobs, JsonRequestBehavior.AllowGet );
		}

		[HttpPost]
		public JsonNetResult CreateNotificationJob( NotificationJob job )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.HotelRepository.CheckHotelPermission( user.PKID, job.FKHotel ).Result != PermissionResults.Authorized )
				return JsonNet( false, JsonRequestBehavior.AllowGet );
			job.FKCreatedByContactUser = user.PKID;
			var newJob = ConnectCmsRepository.NotificationRepository.SaveNotificationJob( job, string.Format( "CU|{0}", user.PKID ) );
			return JsonNet( new
			{
				newJob.PKID,
				newJob.Name,
				newJob.FKHotel,
				newJob.Status,
				newJob.FKCreatedByContactUser,
				newJob.CreatedOn,
				newJob.UpdatedOn,
				newJob.StartDateTime,
				CreatorByContactUser = new
				{
					newJob.ContactUser.PKID,
					newJob.ContactUser.Email,
					newJob.ContactUser.ContactUserName
				}
			}, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult CancelNotificationJob( int jobId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( ConnectCmsRepository.NotificationRepository.CheckNotificationJobPermission( user.PKID, jobId ).Result != PermissionResults.Authorized )
				throw new SecurityException( "You do not have access to this SMS Job" );
			var job = ConnectCmsRepository.NotificationRepository.CancelNotificationJob( user.PKID, jobId );

			return JsonNet( job, JsonRequestBehavior.AllowGet );
		}

		[HttpPost]
		public JsonNetResult KeepAlive()
		{
			return JsonNet( new { keepAlive = true } );
		}

		[HttpPost]
		[AllowAnonymous]
		public JsonNetResult NotifyJobChanged( int NotificationJobId, int NotificationJobStatus, DateTime? LastUpdated )
		{
			CMSHub.NotifyNotificationJobChange( NotificationJobId, NotificationJobStatus, LastUpdated ?? DateTime.UtcNow );
			return JsonNet( true );
		}
	}
}