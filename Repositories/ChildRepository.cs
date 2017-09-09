using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;

namespace ConnectCMS.Repositories
{
	public class ChildRepository : BaseRepository
	{
		private readonly ConnectCMSRepository _rootRepository;

		public ChildRepository( ConnectCMSRepository rootRepository )
			: base()
		{
			_rootRepository = rootRepository;
		}

		public ChildRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context, MonsciergeEntities proxylessContext,
			RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( context, proxylessContext, rp, cacheManager )
		{
			_rootRepository = rootRepository;
		}

		public ConnectCMSRepository RootRepository
		{
			get { return _rootRepository; }
		}

		public new MonsciergeEntities Context
		{
			get { return RootRepository.Context; }
		}

		public new MonsciergeEntities ProxylessContext
		{
			get { return RootRepository.ProxylessContext; }
		}

		public new RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> Rp
		{
			get { return RootRepository.Rp; }
		}

		public new ICacheManager CacheManager
		{
			get { return RootRepository.CacheManager; }
		}
	}
}