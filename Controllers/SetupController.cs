using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Web.Mvc;
using System.Web.Routing;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class SetupController : ControllerBase
	{
		//
		// GET: /Setup/
		public ActionResult Index()
		{
			// This is being disabled for now
			//if( !ConnectCmsRepository.AccountRepository.HasAccountStatusFlag( AccountStatusFlags.IntroCompleted ) )
			//{
			//    return RedirectToAction( "Introduction" );
			//}

			var device = ConnectCmsRepository.DeviceRepository.GetAdminDeviceForLoggedInUser();

			if( device == null )
				return RedirectToAction( "Index", "Error", new RouteValueDictionary( new { header = "ErrorHeader", message = "ErrorMessage" } ) );
			return View( device );
		}

		public ActionResult Introduction()
		{
			//We are hiding this until it has been vetted
			return RedirectToAction( "Index" );
			//if (ConnectCmsRepository.AccountRepository.HasAccountStatusFlag(AccountStatusFlags.IntroCompleted))
			//{
			//    return RedirectToAction("Index");
			//}

			//return View();
		}

		[HttpPost]
		public ActionResult Step2()
		{
			return PartialView( "_setupStep2" );
		}

		[HttpPost]
		public ActionResult Step3()
		{
			return PartialView( "_setupStep3" );
		}

		[HttpPost]
		public ActionResult Step4()
		{
			return PartialView( "_setupStep4" );
		}

		[HttpPost]
		public JsonNetResult FinishIntro()
		{
			ConnectCmsRepository.AccountRepository.AddAccountStatusFlag( AccountStatusFlags.IntroCompleted );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult GetHotelSetup( int? deviceId )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelSetup( deviceId, null );
			return JsonNet( hotel );
		}

		[HttpPost]
		public JsonNetResult UpdateHotelSetup( Hotel model )
		{
			var updatedHotel = ConnectCmsRepository.HotelRepository.UpdateHotelSetup( model );
			return JsonNet( updatedHotel );
		}

		[HttpPost]
		public JsonNetResult FinishSetup()
		{
			ConnectCmsRepository.HotelRepository.CompleteSetup();

			return JsonNet( true );
		}

		[HttpPost]
		public ActionResult MobileBackgroundPreview()
		{
			return PartialView( "Dialogs/_MobileBackgroundPreview" );
		}

		[HttpPost]
		public JsonNetResult MapLogoImage( int deviceId, string path, string name, int width, int height )
		{
			var image = ConnectCmsRepository.ImageRepository.InsertImage( path, name, width, height );
			ConnectCmsRepository.HotelRepository.MapLogoImage( deviceId, image );
			return JsonNet( image );
		}

		[HttpPost]
		public JsonNetResult MapMobileBackgroundImage( int deviceId, string path, string name, int width, int height )
		{
			var image = ConnectCmsRepository.ImageRepository.InsertImage( path, name, width, height );
			ConnectCmsRepository.HotelRepository.MapMobileBackgroundImage( deviceId, image );
			return JsonNet( image );
		}
	}
}