using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;

namespace ConnectCMS.Repositories
{
	public class SalesForceRepository : ChildRepository
	{
		public SalesForceRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public SalesForceRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}
	}
}