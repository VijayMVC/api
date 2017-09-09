using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class UtilityRepository : ChildRepository
	{
		public UtilityRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public UtilityRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public List<Country> GetCountries()
		{
			var countries = GetAllCountries();
			return countries.ToList();
		}

		public List<Country> GetCountriesWithPhoneNumber()
		{
			var countries =
				GetCountries().Where( c => !string.IsNullOrEmpty( c.PhoneNumberCountryCode ) );

			return countries.ToList();
		}

		public List<PostalCode> GetPostalCodes( string name )
		{
			var postalCodes = CacheManager.Get(
				"PostalCodes",
				() => Rp.ExecuteAction( () => ( from pc in ProxylessContext.PostalCodes.Include( pc => pc.Country.States ).Include( pc => pc.State )
												where pc.IsActive
												select pc ) ) );

			return postalCodes.Where( pc => pc.Name == name ).ToList();
		}

		public List<State> GetStates( int? countryId )
		{
			return GetAllStates().Where( s => !countryId.HasValue || s.FKCountry == countryId ).ToList();
		}

		public static IEnumerable<Country> GetAllCountries()
		{
			var cacheManager = new CacheManager( 30 );

			var countries = cacheManager.Get(
				"Countries",
				() =>
				{
					var rp = new RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced>( 10 );
					using( var proxylessContext = new MonsciergeEntities
					{
						CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds,
					} )
					{
						proxylessContext.ObjectContext.ContextOptions.ProxyCreationEnabled = false;
						return
							rp.ExecuteAction( () => from c in proxylessContext.Countries.Where( c2 => c2.IsActive ).Include( c2 => c2.States )
													orderby c.Ordinal
													select c ).ToList();
					}
				} );

			return countries;
		}

		public static IEnumerable<State> GetAllStates()
		{
			var cacheManager = new CacheManager( 30 );
			var states = cacheManager.Get(
				"States",
				() =>
				{
					var rp = new RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced>( 10 );
					using( var proxylessContext = new MonsciergeEntities
					{
						CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds,
					} )
					{
						proxylessContext.ObjectContext.ContextOptions.ProxyCreationEnabled = false;
						return
							rp.ExecuteAction( () => from s in proxylessContext.States.Where( s => s.IsActive )
													orderby s.Ordinal
													select s ).ToList();
					}
				} );

			return states;
		}
	}
}