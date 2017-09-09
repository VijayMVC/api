using Monscierge.Utilities;
using PostSharp.Extensibility;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class CreativeServiceController : ControllerBase
	{
		// GET: CreativeServicez
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult ImageUploader()
		{
			if( !ConnectCmsRepository.SecurityRepository.IsSuperAdmin() )
				return View( "Unauthorized" );

			return View( "ImageUploader" );
		}

		public ActionResult ThemeBuilder()
		{
			if( !ConnectCmsRepository.SecurityRepository.IsSuperAdmin() )
				return View( "Unauthorized" );

			return View( "ThemeBuilder" );
		}
	}
}