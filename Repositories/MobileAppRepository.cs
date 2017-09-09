using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ConnectCMS.Repositories
{
	public class MobileAppRepository : ChildRepository
	{
		public MobileAppRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public MobileAppRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context, MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public MobileApp GetMobileApp( int id, string includes )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();
			var mobileApps = Rp.ExecuteAction( () =>
			{
				var h = ProxylessContext.MobileApps.Where( x => x.PKID == id );
				return includes.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Aggregate( h, ( current, include ) => current.Include( include ) );
			} );

			return mobileApps.FirstOrDefault();
		}

		public List<MobileApp> SearchMobileApp( string searchText, string includes )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();
			var mobileApps = Rp.ExecuteAction( () =>
			{
				var h = ProxylessContext.MobileApps.Where( x => x.Name.ToLower().Contains( searchText.ToLower() ) );
				return includes.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Aggregate( h, ( current, include ) => current.Include( include ) );
			} );
			return mobileApps.ToList();
		}
	}
}