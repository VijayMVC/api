using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;

namespace ConnectCMS.Repositories
{
	public class BaseRepository
	{
		private MonsciergeEntities _context;
		private MonsciergeEntities _proxylessContext;
		private ICacheManager _cacheManager;
		private RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> _rp;

		public BaseRepository()
		{
		}

		public BaseRepository( MonsciergeEntities context, MonsciergeEntities proxylessContext,
			RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
		{
			_context = context;
			_proxylessContext = proxylessContext;
			_rp = rp;
			_cacheManager = cacheManager;
		}

		protected string FormatCulture( string culture )
		{
			string result = null;

			if( !string.IsNullOrWhiteSpace( culture ) && culture.Length == 2 )
				result = culture + "-" + culture.ToUpper();

			return culture;
		}

		public MonsciergeEntities Context
		{
			get
			{
				return _context ?? ( _context = new MonsciergeEntities
				{
					CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds
				} );
			}
		}

		public MonsciergeEntities ProxylessContext
		{
			get
			{
				if( _proxylessContext != null )
					return _proxylessContext;
				_proxylessContext = new MonsciergeEntities
				{
					CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds
				};
				_proxylessContext.ObjectContext.ContextOptions.ProxyCreationEnabled = false;
				return _proxylessContext;
			}
		}

		public RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> Rp
		{
			get
			{
				return _rp ?? ( _rp =
					new RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced>( 10 ) );
			}
		}

		public ICacheManager CacheManager
		{
			get { return _cacheManager ?? ( _cacheManager = new CacheManager( 30 ) ); }
		}
	}
}