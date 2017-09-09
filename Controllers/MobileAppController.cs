using Monscierge.Utilities;
using MonsciergeDataModel;
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
	public class MobileAppController : ControllerBase
	{
		[HttpPost]
		public JsonNetResult SearchMobileApps( string searchText, string includes )
		{
			var mobileApps = new List<MobileApp>();
			int id;
			if( int.TryParse( searchText, out id ) )
			{
				var mobileApp = ConnectCmsRepository.MobileAppRepository.GetMobileApp( id, includes );
				if( mobileApp != null )
					mobileApps.Add( mobileApp );
			}
			else
			{
				mobileApps = ConnectCmsRepository.MobileAppRepository.SearchMobileApp( searchText, includes );
			}

			return JsonNet( mobileApps );
		}
	}
}