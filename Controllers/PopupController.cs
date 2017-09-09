using ConnectCMS.Models.Popup;
using Monscierge.Utilities;
using PostSharp.Extensibility;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class PopupController : ControllerBase
	{
		// GET: /Dialog/
		public ActionResult Index( string view )
		{
			return View( view );
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult Alert( string header, string body, string stackTrace, string button )
		{
			var vm = new AlertViewModel
			{
				Header = header,
				Body = body,
				StackTrace = stackTrace,
				Button = button
			};

			return PartialView( "_Alert", vm );
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult Dialog( string header, string body, string okButton, string cancelButton )
		{
			var vm = new DialogViewModel
			{
				Header = header,
				Body = body,
				OkButton = okButton,
				CancelButton = cancelButton
			};

			return PartialView( "_Dialog", vm );
		}

		[AllowAnonymous]
		public ActionResult AboutCMS()
		{
			return PartialView( "_AboutCMS" );
		}

		[AllowAnonymous]
		public ActionResult Feedback()
		{
			return PartialView( "_Feedback" );
		}

		[AllowAnonymous]
		public ActionResult Languages()
		{
			return PartialView( "_Languages" );
		}

		[AllowAnonymous]
		public ActionResult BrowserUnsupported()
		{
			return PartialView( "_BrowserUnsupported" );
		}

		[AllowAnonymous]
		public ActionResult AlreadySignedIn()
		{
			return PartialView( "_AlreadySignedIn" );
		}

		[AllowAnonymous]
		public ActionResult InactiveDialog()
		{
			return PartialView( "_InactiveDialog" );
		}

		[HttpPost]
		public ActionResult PreviewImage( int imageId )
		{
			var image = ConnectCmsRepository.ImageRepository.GetImage( imageId );
			return PartialView( "_PreviewImage", image );
		}
	}
}