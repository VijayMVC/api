using ConnectCMS.Models.Enterprise;
using ConnectCMS.Models.Location;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Utils;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class BingRepository : ChildRepository
	{
		public BingRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public BingRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		protected string GetBingAppId()
		{
			return ConfigurationManager.AppSettings.Get( "BingAppId" );
		}

		public DbGeography GetBingAddressGeocode( string address1, string city, string state, string postalCode, string country, string culture )
		{
			DbGeography result = null;

			var address = new Bing.Geocode.Address()
			{
				AddressLine = address1,
				AdminDistrict = state,
				CountryRegion = country,
				Locality = city,
				PostalCode = postalCode
			};
			var bingAppId = GetBingAppId();

			var geocodeRequest = new Bing.Geocode.GeocodeRequest()
			{
				Address = address,
				Credentials = new Bing.Geocode.Credentials()
				{
					ApplicationId = bingAppId
				},
				Culture = FormatCulture( culture ),
				Options = new Bing.Geocode.GeocodeOptions()
				{
					Count = 1,
					Filters = new List<Bing.Geocode.FilterBase>() {
						new Bing.Geocode.ConfidenceFilter()
						{
							MinimumConfidence = Bing.Geocode.Confidence.Medium
						}
					}
				}
			};

			Bing.Geocode.GeocodeServiceClient geocodeServiceClient = new Bing.Geocode.GeocodeServiceClient( "BasicHttpBinding_IGeocodeService" );

			var geocodeResponse = geocodeServiceClient.Geocode( geocodeRequest );

			if( geocodeResponse != null && geocodeResponse.Results != null && geocodeResponse.Results.Count() > 0 )
			{
				var geocodeResult = geocodeResponse.Results.FirstOrDefault();

				if( geocodeResult != null )
				{
					var geocodeLocation = geocodeResult.Locations.FirstOrDefault();

					if( geocodeLocation != null && geocodeLocation.Latitude != 0 && geocodeLocation.Longitude != 0 )
						result = GeographyUtils.GetPointFromLatitudeAndLongitude( ( float )geocodeLocation.Latitude,
							( float )geocodeLocation.Longitude );
				}
			}

			return result;
		}

		public List<EnterpriseModel> GetSearchBingEnterprises( float? hotelLatitude, float? hotelLongitude, float radiusInKilometers, string text, EnterpriseSearchSortTypes sortType, string culture, IEnumerable<string> excludeBingIds = null )
		{
			List<EnterpriseModel> enterprises = new List<EnterpriseModel>();

			if( hotelLatitude.HasValue && hotelLongitude.HasValue )
			{
				var bingAppId = GetBingAppId();
				var enUsCultureInfo = new CultureInfo( "en-US" );
				var latitude = hotelLatitude.Value;
				var longitude = hotelLongitude.Value;

				var searchRequest = new Bing.Search.SearchRequest()
				{
					Credentials = new Bing.Search.Credentials()
					{
						ApplicationId = bingAppId
					},
					Culture = FormatCulture( culture ),
					SearchOptions = new Bing.Search.SearchOptions()
					{
						Count = 20,
						ListingType = Bing.Search.ListingType.Business,
						Radius = radiusInKilometers,
						SortOrder = Bing.Search.SortOrder.Popularity
					},
					StructuredQuery = new Bing.Search.StructuredSearchQuery()
					{
						Location = latitude.ToString( enUsCultureInfo ) + ", " + longitude.ToString( enUsCultureInfo ),
						Keyword = text
					},
					UserProfile = new Bing.Search.UserProfile()
					{
						CurrentLocation = new Bing.Search.UserLocation()
						{
							Latitude = latitude,
							Longitude = longitude
						}
					}
				};

				var searchServiceService = new Bing.Search.SearchServiceClient( "BasicHttpBinding_ISearchService" );
				var searchResponse = searchServiceService.Search( searchRequest );

				if( searchResponse != null &&
					searchResponse.ResultSets != null && searchResponse.ResultSets.Count() > 0 &&
					searchResponse.ResultSets[ 0 ].Results != null && searchResponse.ResultSets[ 0 ].Results.Count() > 0 )
				{
					Bing.Search.Address address;
					Bing.Search.BusinessSearchResult businessSearchResult;
					EnterpriseLocationModel enterpriseLocation;
					EnterpriseModel enterprise;
					Bing.Search.GeocodeLocation geocodeLocation;

					foreach( Bing.Search.SearchResultBase searchResultBase in searchResponse.ResultSets[ 0 ].Results )
					{
						if( searchResultBase is Bing.Search.BusinessSearchResult &&
							searchResultBase.LocationData != null &&
							searchResultBase.LocationData.Locations != null && searchResultBase.LocationData.Locations.Count > 0 )
						{
							businessSearchResult = searchResultBase as Bing.Search.BusinessSearchResult;

							if( excludeBingIds == null || !excludeBingIds.Contains( businessSearchResult.Id ) )
							{
								enterprise = new EnterpriseModel()
								{
									EnterpriseLocations = new List<EnterpriseLocationModel>(),
									Name = businessSearchResult.Name,
									WebsiteUrl = businessSearchResult.Website != null ? businessSearchResult.Website.ToString() : null
								};

								enterpriseLocation = new EnterpriseLocationModel()
								{
									BingId = businessSearchResult.Id,
									Blacklisted = false,
									DistanceInKilometers = businessSearchResult.Distance,
									LocalHours = businessSearchResult.AdditionalProperties.ContainsKey( "HoursOfOperation" ) ? businessSearchResult.AdditionalProperties[ "HoursOfOperation" ].ToString().ToLower() : null,
									Phone = businessSearchResult.PhoneNumber
								};

								geocodeLocation = searchResultBase.LocationData.Locations.FirstOrDefault();

								address = businessSearchResult.Address;

								if( address != null )
								{
									enterpriseLocation.Location = new LocationModel()
									{
										Address1 = address.AddressLine,
										City = address.PostalTown ?? address.Locality,
										ISOCountryCode = address.CountryRegion,
										PostalCode = address.PostalCode,
										ISOStateCode = address.AdminDistrict
									};

									if( geocodeLocation.Latitude != 0 && geocodeLocation.Longitude != 0 )
										enterpriseLocation.Location.Latitude = ( float )geocodeLocation.Latitude;
									enterpriseLocation.Location.Longitude = ( float )geocodeLocation.Longitude;

									enterpriseLocation.PhoneISOCountryCode = enterpriseLocation.Location.ISOCountryCode;
								}

								// TODO: JD: Get culture
								enterpriseLocation.TranslatedHours.Add( "EN", enterpriseLocation.LocalHours );

								( ( List<EnterpriseLocationModel> )enterprise.EnterpriseLocations ).Add( enterpriseLocation );

								enterprises.Add( enterprise );
							}
						}
					}
				}

				if( enterprises.Any() )
					switch( sortType )
					{
						case EnterpriseSearchSortTypes.ALAPHABETICAL:
							enterprises = enterprises.OrderBy( e => e.Name ).ThenBy( e => e.EnterpriseLocations.Min( el => el.DistanceInMeters ) ).ToList();

							break;

						case EnterpriseSearchSortTypes.NEAREST:
							enterprises = enterprises.OrderBy( e => e.EnterpriseLocations.Min( el => el.DistanceInMeters ) ).ToList();

							break;
					}
			}

			return enterprises;
		}

		public object UpdateBingAddressGeocode( Location location, string culture )
		{
			DbGeography result = null;

			var address = new Bing.Geocode.Address()
			{
				AddressLine = location.Address1,
				AdminDistrict = location.ISOStateCode,
				CountryRegion = location.ISOCountryCode,
				Locality = location.City,
				PostalCode = location.PostalCode
			};

			var bingAppId = GetBingAppId();

			var geocodeRequest = new Bing.Geocode.GeocodeRequest()
			{
				Address = address,
				Credentials = new Bing.Geocode.Credentials()
				{
					ApplicationId = bingAppId
				},
				Culture = FormatCulture( culture ),
				Options = new Bing.Geocode.GeocodeOptions()
				{
					Count = 1,
					Filters = new List<Bing.Geocode.FilterBase>() {
						new Bing.Geocode.ConfidenceFilter()
						{
							MinimumConfidence = Bing.Geocode.Confidence.Medium
						}
					}
				}
			};

			var geocodeServiceClient = new Bing.Geocode.GeocodeServiceClient( "BasicHttpBinding_IGeocodeService" );

			var geocodeResponse = geocodeServiceClient.Geocode( geocodeRequest );

			if( geocodeResponse != null && geocodeResponse.Results != null && geocodeResponse.Results.Any() )
			{
				var geocodeResult = geocodeResponse.Results.FirstOrDefault();

				if( geocodeResult != null )
				{
					var geocodeLocation = geocodeResult.Locations.FirstOrDefault();

					if( geocodeLocation != null && geocodeLocation.Latitude != 0 && geocodeLocation.Longitude != 0 )
						result = GeographyUtils.GetPointFromLatitudeAndLongitude( ( float )geocodeLocation.Latitude,
							( float )geocodeLocation.Longitude );
				}
			}
			location.Latitude = ( float )( result != null ? result.Latitude ?? 0 : 0 );
			location.Longitude = ( float )( result != null ? result.Longitude ?? 0 : 0 );
			return location;
		}
	}
}