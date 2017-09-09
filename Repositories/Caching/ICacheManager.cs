using System;

namespace ConnectCMS.Repositories.Caching
{
	public interface ICacheManager
	{
		void Clear<T>( string cacheKey ) where T : class;

		T Get<T>( string cacheKey, Func<T> notFoundCallback ) where T : class;
	}
}