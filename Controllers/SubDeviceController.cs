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
	public class SubDeviceController : ControllerBase
	{
		public ActionResult Index()
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( user.DefaultReachRole.Name != "Super Admin" )
				return View( "Unauthorized" );

			return View();
		}

		public ActionResult ConfigureReaderboards( int? deviceId )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			if( !deviceId.HasValue )
			{
				var device = ConnectCmsRepository.DeviceRepository.GetDeviceForLoggedInUser();

				if( device != null )
				{
					ViewBag.DeviceId = device.PKID;
				}
				else
				{
				}
			}
			else
			{
			}
			return View();
		}

		[HttpPost]
		public JsonNetResult GetReaderboards( int deviceId )
		{
			var results = ConnectCmsRepository.SubDeviceRepository.GetReaderboards( deviceId );
			return JsonNet( results.Select( x => new { x.PKID, x.Name, x.FKReaderboardBackgroundImage, x.ReaderboardBackgroundImage } ) );
		}
	}
}