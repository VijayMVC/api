using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class ImageController : ControllerBase
	{
		// GET: /Image/Reorder
		public ActionResult Reorder()
		{
			return PartialView( "_Reorder" );
		}

		// GET: /Image/Preview
		public ActionResult Preview()
		{
			return PartialView( "_Preview" );
		}

		public JsonNetResult DeleteEnterpriseImage( int deviceId, int enterpriseId, int imageId )
		{
			//ConnectCmsRepository.ImageRepository.DeleteHotelCategory( deviceId, categoryId );

			return JsonNet( true );
		}

		public JsonNetResult InsertImage( string path, string name, int width, int height )
		{
			var image = ConnectCmsRepository.ImageRepository.InsertImage( path, name, width, height );
			return JsonNet( image );
		}
	}
}