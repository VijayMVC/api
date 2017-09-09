using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class TipController : ControllerBase
	{
		// GET: /Tip/Add
		public ActionResult Add()
		{
			return PartialView( "_Add" );
		}

		// POST: /Tip/DeleteTip
		[HttpPost]
		public JsonNetResult DeleteTip( int deviceId, int tipId )
		{
			ConnectCmsRepository.TipRepository.DeleteTip( deviceId, tipId );

			return JsonNet( true );
		}

		// POST: /Tip/InsertTip
		[HttpPost]
		public JsonNetResult InsertTip( int deviceId, string tip, int enterpriseId, int? enterpriseLocationId = null )
		{
			int userId = ConnectCmsRepository.SecurityRepository.GetLoggedInUser().PKID;

			if( !enterpriseLocationId.HasValue )
				ConnectCmsRepository.TipRepository.InsertEnterpriseTip( deviceId, userId, tip, enterpriseId );
			else
				ConnectCmsRepository.TipRepository.InsertEnterpriseLocationTip( deviceId, userId, tip, enterpriseId, enterpriseLocationId.Value );

			return JsonNet( true );
		}

		// POST: /Tip/UpdateTip
		[HttpPost]
		public JsonNetResult UpdateTip( int deviceId, int tipId, string tip )
		{
			int userId = ConnectCmsRepository.SecurityRepository.GetLoggedInUser().PKID;

			ConnectCmsRepository.TipRepository.UpdateTip( deviceId, tipId, tip, userId );

			return JsonNet( true );
		}
	}
}