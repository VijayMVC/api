using AutoMapper;
using ConnectCMS.Models;
using ConnectCMS.ViewModels.Dashboard;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	[Authorize]
	public class DashboardController : ControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult TestLanding()
		{
			return View();
		}

		public ActionResult GetTodaysRequestCount( int deviceId )
		{
			var device = ConnectCmsRepository.DeviceRepository.GetDevice( deviceId );
			if( device == null )
				return new HttpNotFoundResult();

			var count = ConnectCmsRepository.DashboardRepository.GetTodaysRequestCount( device.FKHotel );

			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public ActionResult GetTodaysPostcardCount( int deviceId )
		{
			var count = ConnectCmsRepository.DashboardRepository.GetTodaysPostcardCount( deviceId );
			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public ActionResult GetRecommendationCount( int deviceId )
		{
			var count = ConnectCmsRepository.DashboardRepository.GetCurrentRecommendationCountForDevice( deviceId );
			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public ActionResult GetTomorrowsEventCount( int? deviceId )
		{
			var count = ConnectCmsRepository.DashboardRepository.GetTomorrowsEventCount( deviceId );

			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult GetTodaysDashboardEvents( int? deviceId )
		{
			var events = ConnectCmsRepository.DashboardRepository.GetTodaysEvents( deviceId );
			var viewModel = Mapper.Map<List<DashboardEventViewModel>>( events );

			return new JsonNetResult
			{
				Data = viewModel,
				SerializerSettings = new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver()
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		public ActionResult GetGuestInteractionTodayCount( int deviceId )
		{
			var count = ConnectCmsRepository.DashboardRepository.GetUserInteractionTodayCount( deviceId );
			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public ActionResult GetGuestInteraction10DayAvgCount( int deviceId )
		{
			var count = ConnectCmsRepository.DashboardRepository.GetUserInteraction10DayAvgCount( deviceId );
			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public ActionResult GetTodaysInRoomDiningCount( int deviceId )
		{
			var count = ConnectCmsRepository.DashboardRepository.GetTodaysInRoomDiningCount( deviceId );
			return JsonNet( new { count }, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult SetDashboardBeacon( int beaconId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			var beacon = ConnectCmsRepository.BeaconRepository.GetBeacon( beaconId );
			if( beacon == null )
				return JsonNet( null, JsonRequestBehavior.AllowGet );

			if( ConnectCmsRepository.BeaconRepository.CheckBeaconPermission( user.PKID, beaconId ).Result == PermissionResults.Authorized )
			{
				HttpContext.Response.SetCookie( new HttpCookie( "DashboardBeacon", beaconId.ToString() ) );
				return JsonNet( true );
			}
			else
			{
				return JsonNet( false );
			}
		}

		public JsonNetResult GetBeacons( int deviceId )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );
			if( hotel == null )
				return JsonNet( new List<Beacon>(), JsonRequestBehavior.AllowGet );
			var beacons = ConnectCmsRepository.BeaconRepository.GetBeacons( hotel.PKID ).ToList();
			return JsonNet( beacons, JsonRequestBehavior.AllowGet );
		}

		public JsonNetResult GetBeaconNearbyUsers( int? beaconId )
		{
			if( !beaconId.HasValue )
			{
				int id;
				var cookie = HttpContext.Request.Cookies.Get( "DashboardBeacon" );
				if( cookie == null || !int.TryParse( cookie.Value, out id ) )
					return JsonNet( new User[ 0 ], JsonRequestBehavior.AllowGet );
				beaconId = id;
			}

			var users = ConnectCmsRepository.BeaconRepository.GetNearbyUsers( beaconId.Value, new TimeSpan( 0, 10, 0 ), true ).ToList();
			return JsonNet( users, JsonRequestBehavior.AllowGet );
		}
	}
}