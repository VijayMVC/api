using ConnectCMS.Models.Analytic;
using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public, ApplyToStateMachine = false )]
	public class AnalyticController : ControllerBase
	{
		// GET: Analytic
		public ActionResult RequestAnalytics( int? deviceId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( !deviceId.HasValue )
			{
				var device = ConnectCmsRepository.DeviceRepository.GetAdminDeviceForLoggedInUser();

				if( device != null )
				{
					ViewBag.DeviceId = device.PKID;
					deviceId = device.PKID;
				}
			}

			var model = new RequestAnalyticModel
			{
				JsonParameters = deviceId.HasValue ? ConnectCmsRepository.AnalyticRepository.GetJsonAnalyticParameters( deviceId.Value ) : ""
			};

			return View( model );
		}

		[HttpPost]
		public JsonNetResult GetRequestReport( int? deviceId, DateTime fromDate, DateTime toDate, int? requestCategory, int? requestType, int? requestUser,
			string guestName, string roomNumber )
		{
			var results = ConnectCmsRepository.AnalyticRepository.GetRequestReport( deviceId, fromDate.ToUniversalTime(), toDate.ToUniversalTime(), requestCategory, requestType,
				requestUser,
				guestName, roomNumber );

			return JsonNet( results );
		}
	}
}