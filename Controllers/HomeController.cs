using ConnectCMS.Models;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class HomeController : ControllerBase
	{
		public ActionResult Index()
		{
			if( ConnectCmsRepository.SecurityRepository.IsEventManager() )
				return RedirectToAction( "Index", "Events" );

			if( ConnectCmsRepository.SecurityRepository.IsCampaignManager() )
				return RedirectToAction( "Index", "MarketingCampaign" );

			if( !ConnectCmsRepository.HotelRepository.HasCompletedSetup() )
			{
				return RedirectToAction( "Index", "Setup" );
			}

			ViewBag.AllowDeviceNetwork = true;

			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
			ViewBag.ManageUsers = user.DefaultReachRole.InviteNewManagers;

			return View( new HomeViewModel() { CurrentUser = ConnectCmsRepository.SecurityRepository.GetLoggedInUser() } );
		}

		#region Navigation

		[HttpPost]
		public JsonNetResult LoadNavigationItems( int? deviceId )
		{
			var menuItems = ConnectCmsRepository.SecurityRepository.GetNavigationMenuItems( deviceId );

			return JsonNet( menuItems );
		}

		#endregion Navigation
	}
}