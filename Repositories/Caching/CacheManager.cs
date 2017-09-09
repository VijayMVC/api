using System;
using System.Collections.Concurrent;
using System.Web;
using System.Web.Caching;

namespace ConnectCMS.Repositories.Caching
{
	public class CacheManager : ICacheManager
	{
		private const int DefaultCacheTimeInMinutes = 60;
		private static readonly ConcurrentDictionary<string, object> LockObjects = new ConcurrentDictionary<string, object>();
		private readonly int _cacheTimeInMinutes;

		public CacheManager()
		{
			_cacheTimeInMinutes = DefaultCacheTimeInMinutes;
		}

		public CacheManager( int cacheMinutes )
		{
			_cacheTimeInMinutes = ( cacheMinutes >= 0 ) ? cacheMinutes : DefaultCacheTimeInMinutes;
		}

		public void Clear<T>( string cacheKey ) where T : class
		{
			var item = HttpRuntime.Cache.Get( cacheKey ) as T;
			if( item == null )
				return;

			HttpRuntime.Cache.Remove( cacheKey );
		}

		public T Get<T>( string cacheKey, Func<T> notFoundCallback ) where T : class
		{
			var item = HttpRuntime.Cache.Get( cacheKey ) as T;
			if( item != null )
				return item;

			object keyedLockObject;
			if( !LockObjects.TryGetValue( cacheKey, out keyedLockObject ) )
			{
				keyedLockObject = new object();
				LockObjects.TryAdd( cacheKey, keyedLockObject );
			}

			lock( keyedLockObject )
			{
				item = HttpRuntime.Cache.Get( cacheKey ) as T;
				if( item != null )
					return item;

				item = notFoundCallback();
				HttpContext.Current.Cache.Insert( cacheKey, item, null, DateTime.Now.AddMinutes( _cacheTimeInMinutes ),
					Cache.NoSlidingExpiration );
			}
			return item;
		}
	}
}