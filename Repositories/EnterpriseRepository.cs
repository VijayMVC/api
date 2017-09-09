#region using

using ConnectCMS.Models.ContactUser;
using ConnectCMS.Models.Enterprise;
using ConnectCMS.Models.Image;
using ConnectCMS.Models.Location;
using ConnectCMS.Models.Recommendation;
using ConnectCMS.Models.Utility;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Utils;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

#endregion using

namespace ConnectCMS.Repositories
{
	public class EnterpriseRepository : ChildRepository
	{
		public EnterpriseRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public EnterpriseRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp,
			ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public EnterpriseModel GetEnterpriseForEdit( int deviceId, int enterpriseId, int? enterpriseLocationId,
			float? hotelLatitude, float? hotelLongitude, float radiusInMeters )
		{
			EnterpriseModel enterprise = null;

			var hotelPoint = GeographyUtils.GetPointFromLatitudeAndLongitude( hotelLatitude, hotelLongitude );

			if( hotelPoint != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				// TODO: JD: Culture.
				var culture = MvcApplication.GetCurrentCulture();

				var nativeLanguage = culture.Substring( 0, 2 ).ToUpper();

				var item =
					Rp.ExecuteAction(
						() => ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive && e2.PKID == enterpriseId )
								let eim = e.EnterpriseImageMaps.Where( eim2 => eim2.IsActive )
								let el = e.EnterpriseLocations.Where( el2 => el2.IsActive && ( !enterpriseLocationId.HasValue || el2.PKID == enterpriseLocationId.Value ) )
								let ecm = e.EnterpriseCategoryMaps
								let fecm = ecm.FirstOrDefault()
								let cm = ecm.Where( ecm2 => ecm2.CategoryMap.IsActive ).Select( ecm2 => ecm2.CategoryMap )
								let fcm = cm.FirstOrDefault()
								let hcm = cm.SelectMany( cm2 => cm2.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive && hcm2.FKDevice == deviceId ) )
								let fhcm = hcm.OrderBy( hcm2 => hcm2.Ordinal ).FirstOrDefault()
								let hboem = ecm.SelectMany( ecm2 => ecm2.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive && hboem2.FKDevice == deviceId ).OrderBy( hboem2 => hboem2.Ordinal ) )
								let hboelm = hboem.SelectMany( hboem2 => hboem2.HotelBestOfEnterpriseLocationMaps )
								select new
								{
									CMImage = fcm.Image,
									ECMImage = fecm.Image,
									Enterprise = new EnterpriseModel
									{
										EnterpriseLocations = ( from el2 in el
																let c = ProxylessContext.Countries.FirstOrDefault( c2 => c2.IsActive && c2.ISOCountryCode == el2.Country )
																let s =
																	ProxylessContext.States.FirstOrDefault(
																		s2 => s2.IsActive && s2.ISOStateCode == el2.State && s2.FKCountry == c.PKID )
																select new EnterpriseLocationModel
																{
																	BingId = el2.BingId,
																	DistanceInMeters = el2.Coordinates.Distance( hotelPoint ),
																	HourType = el2.HourType,
																	LocalHours = el2.Hours,
																	LocalHoursLanguage = !string.IsNullOrEmpty( el2.HoursLanguage ) ? el2.HoursLanguage : nativeLanguage,
																	LocalHoursXml = el2.LocalizedHours,
																	Location = new LocationModel
																	{
																		Address1 = el2.Address1,
																		Address2 = el2.Address2,
																		City = el2.City,
																		Country = c,
																		Latitude = el2.Latitude,
																		Longitude = el2.Longitude,
																		PostalCode = el2.PostalCode,
																		State = s
																	},
																	Phone = el2.Phone,
																	PhoneISOCountryCode = el2.PhoneISOCountryCode,
																	PKID = el2.PKID,
																	Recommended = hboelm.Any( hboelm2 => hboelm2.FKEnterpriseLocation == el2.PKID )
																} ),
										LocalDescription = e.Description,
										LocalDescriptionLanguage =
											e.DescriptionLanguage != null && e.DescriptionLanguage != string.Empty
												? e.DescriptionLanguage
												: nativeLanguage,
										LocalDescriptionXml = e.LocalizedDescription,
										Name = e.Name,
										PKID = e.PKID,
										Recommended = hboem.Any() && !hboelm.Any()
									},
									EnterpriseImages = eim.Select( eim2 => eim2.Image ),
									HCMImage = fhcm.Image,
								} ) ).FirstOrDefault();

				if( item != null && item.Enterprise != null )
				{
					if( item.EnterpriseImages != null )
						foreach( var customImage in item.EnterpriseImages )
							( ( List<ImageModel> )item.Enterprise.Images ).Add( new ImageModel( customImage ) );

					if( item.CMImage != null )
						item.Enterprise.ListItemImage = new ImageModel( item.CMImage );

					if( item.HCMImage != null )
						item.Enterprise.ListItemImage = new ImageModel( item.HCMImage );

					if( item.ECMImage != null )
						item.Enterprise.ListItemImage = new ImageModel( item.ECMImage );

					( ( List<ImageModel> )item.Enterprise.Images ).Insert( 0, item.Enterprise.ListItemImage );

					item.Enterprise.Description = Localization.GetLocalizedText( item.Enterprise.LocalDescription,
						item.Enterprise.LocalDescriptionXml, item.Enterprise.LocalDescriptionLanguage, nativeLanguage );
					item.Enterprise.EnterpriseLocations.ForEach(
						el => el.Hours = Localization.GetLocalizedText( el.LocalHours, el.LocalHoursXml, el.LocalHoursLanguage, nativeLanguage ) );

					enterprise = item.Enterprise;
				}
			}

			return enterprise;
		}

		public EnterpriseModel GetEnterpriseRecommendationForEdit( int deviceId, int enterpriseId, int? enterpriseLocationId )
		{
			EnterpriseModel enterprise = null;

			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			enterprise =
				Rp.ExecuteAction( () => ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive && e2.PKID == enterpriseId )
										  let el =
											  e.EnterpriseLocations.Where( el2 => !enterpriseLocationId.HasValue || el2.PKID == enterpriseLocationId.Value )
										  let ecm = e.EnterpriseCategoryMaps
										  let hboem =
											  ecm.SelectMany(
												  ecm2 =>
													  ecm2.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive && hboem2.FKDevice == deviceId )
														  .OrderBy( hboem2 => hboem2.Ordinal ) )
										  let hboelm = hboem.SelectMany( hboem2 => hboem2.HotelBestOfEnterpriseLocationMaps )
										  let blem = e.BlackListEnterpriseMaps.Where( blem2 => blem2.IsActive && blem2.FKDevice == deviceId )
										  let blelm =
											  el.SelectMany(
												  el2 =>
													  el2.BlackListEnterpriseLocationMaps.Where(
														  blelm2 => blelm2.IsActive && blelm2.Hotel.Devices.Any( d => d.PKID == deviceId ) ) )
										  select new EnterpriseModel
										  {
											  Blacklisted = blem.Any(),
											  EnterpriseLocations = ( from el2 in el
																	  let c = ProxylessContext.Countries.FirstOrDefault( c2 => c2.IsActive && c2.ISOCountryCode == el2.Country )
																	  where !hboelm.Any()
																			|| hboelm.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID )
																	  select new EnterpriseLocationModel
																	  {
																		  Blacklisted = blelm.Select( blelm2 => blelm2.FKEnterpriseLocation ).Contains( el2.PKID ),
																		  PKID = el2.PKID,
																		  RecommendedCategories = ( from hboelm2 in hboelm
																									select new OrderModel
																									{
																										Key = hboelm2.HotelBestOfEnterpriseMap.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID,
																										Order = hboelm2.Ordinal
																									} )
																	  } ),
											  PKID = e.PKID,
											  RecommendedCategories = ( from hboem2 in hboem
																		select new OrderModel
																		{
																			Key = hboem2.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID,
																			Order = hboem2.Ordinal
																		} )
										  } ) ).FirstOrDefault();

			return enterprise;
		}

		public EnterpriseModel GetEnterpriseTipForEdit( int deviceId, int enterpriseId, int? enterpriseLocationId )
		{
			EnterpriseModel enterprise = null;

			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			var culture = "en-US";

			var nativeLanguage = culture.Substring( 0, 2 ).ToUpper();

			enterprise =
				Rp.ExecuteAction( () => ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive && e2.PKID == enterpriseId )
										  let el =
											  e.EnterpriseLocations.Where(
												  el2 => el2.IsActive && ( !enterpriseLocationId.HasValue || el2.PKID == enterpriseLocationId.Value ) )
										  let it =
											  e.InsiderTips.Where(
												  it2 =>
													  it2.Hotel.HotelDetail.IsActive &&
													  it2.Hotel.Devices.Where( d => d.DeviceDetail.IsActive ).Select( d => d.PKID ).Contains( deviceId ) && it2.IsActive )
										  select new EnterpriseModel
										  {
											  EnterpriseLocations = ( from el2 in el
																	  let c = ProxylessContext.Countries.FirstOrDefault( c2 => c2.IsActive && c2.ISOCountryCode == el2.Country )
																	  let s = ProxylessContext.States.FirstOrDefault( s => s.IsActive && s.FKCountry == c.PKID )
																	  select new EnterpriseLocationModel
																	  {
																		  Location = new LocationModel
																		  {
																			  Address1 = el2.Address1,
																			  Address2 = el2.Address2,
																			  City = el2.City,
																			  Country = c,
																			  PostalCode = el2.PostalCode,
																			  State = s,
																		  },
																		  PKID = el2.PKID,
																		  Tips = ( from it2 in it
																				   let cu = it2.ContactUser
																				   where it2.FKEnterpriseLocation.HasValue
																						 && it2.FKEnterpriseLocation.Value == el2.PKID
																				   select new TipModel
																				   {
																					   ContactUser = new ContactUserModel
																					   {
																						   Name = cu.ContactUserName,
																						   PKID = cu.PKID
																					   },
																					   LastModified = it2.TipDateTime,
																					   LocalTip = it2.Tip,
																					   LocalTipLanguage =
																						   it2.TipLanguage != null && it2.TipLanguage != string.Empty ? it2.TipLanguage : nativeLanguage,
																					   LocalTipXml = it2.LocalizedTip,
																					   PKID = it2.PKID
																				   } )
																	  } ),
											  PKID = e.PKID,
											  Tips = ( from it2 in it
													   let cu = it2.ContactUser
													   where !it2.FKEnterpriseLocation.HasValue
													   select new TipModel
													   {
														   ContactUser = new ContactUserModel
														   {
															   Name = cu.ContactUserName,
															   PKID = cu.PKID
														   },
														   LastModified = it2.TipDateTime,
														   LocalTip = it2.Tip,
														   LocalTipLanguage = it2.TipLanguage != null && it2.TipLanguage != string.Empty ? it2.TipLanguage : nativeLanguage,
														   LocalTipXml = it2.LocalizedTip,
														   PKID = it2.PKID
													   } )
										  } ) ).FirstOrDefault();

			if( enterprise != null )
			{
				enterprise.Tips.ForEach( t => t.Tip = Localization.GetLocalizedText( t.LocalTip, t.LocalTipXml, t.LocalTipLanguage, nativeLanguage ) );
				enterprise.EnterpriseLocations.ForEach(
					el => el.Tips.ForEach( t => t.Tip = Localization.GetLocalizedText( t.LocalTip, t.LocalTipXml, t.LocalTipLanguage, nativeLanguage ) ) );
			}

			return enterprise;
		}

		public List<EnterpriseModel> GetSearchEnterprises( int deviceId, float? hotelLatitude, float? hotelLongitude, float radiusInMeters,
			int? categoryId, string text, EnterpriseSearchSortTypes sortType )
		{
			List<EnterpriseModel> enterprises = null;

			var device = RootRepository.DeviceRepository.GetDevice( deviceId );

			var hotelPoint = GeographyUtils.GetPointFromLatitudeAndLongitude( hotelLatitude, hotelLongitude );

			if( hotelPoint != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				if( text != null )
					text = text.Trim();

				var query = ( from e in ProxylessContext.Enterprises
					 .Include( x => x.EnterpriseLocations.Select( el => el.BlackListEnterpriseLocationMaps ) )
					 .Include( x => x.EnterpriseLocations.Select( el => el.HotelBestOfEnterpriseLocationMaps ) )
					 .Include( x => x.EnterpriseCategoryMaps.Select( ecm => ecm.Image ) )
					 .Include( x => x.EnterpriseCategoryMaps.Select( ecm => ecm.CategoryMap.Image ) )
					 .Include( x => x.EnterpriseCategoryMaps.Select( ecm => ecm.CategoryMap.HotelCategoryMaps.Select( hcm => hcm.Image ) ) )
					 .Include(
						 x =>
							 x.EnterpriseCategoryMaps.Select(
								 ecm => ecm.HotelBestOfEnterpriseMaps.Select( hboem => hboem.HotelBestOfEnterpriseLocationMaps ) ) )
					 .Include( x => x.BlackListEnterpriseMaps )
							  where
								  e.IsActive &&
								  e.EnterpriseLocations.Any( x => x.IsActive && x.Latitude != null && x.Longitude != null )
							  select new
							  {
								  CMImage = e.EnterpriseCategoryMaps.FirstOrDefault().CategoryMap.Image,
								  ECMImage = e.EnterpriseCategoryMaps.FirstOrDefault().Image,
								  Enterprise = new EnterpriseModel
								  {
									  Blacklisted = e.BlackListEnterpriseMaps.Any( x => x.FKDevice == deviceId ),
									  EnterpriseLocations = e.EnterpriseLocations.Where( x => x.IsActive && x.Latitude != null && x.Longitude != null ).Select( el => new EnterpriseLocationModel
									  {
										  BingId = el.BingId,
										  Blacklisted = el.BlackListEnterpriseLocationMaps.Any( x => x.FKHotel == device.FKHotel ),
										  DistanceInMeters = el.Coordinates.Distance( hotelPoint ),
										  Location = new LocationModel
										  {
											  Address1 = el.Address1,
											  Address2 = el.Address2,
											  City = el.City,
											  ISOCountryCode = el.Country,
											  Latitude = el.Latitude,
											  Longitude = el.Longitude,
											  PostalCode = el.PostalCode,
											  ISOStateCode = el.State
										  },
										  Phone = el.Phone,
										  PKID = el.PKID,
										  Recommended = el.HotelBestOfEnterpriseLocationMaps.Any( x => x.HotelBestOfEnterpriseMap.FKDevice == deviceId )
									  } ),
									  Name = e.Name,
									  PKID = e.PKID,
									  Recommended =
										  e.EnterpriseCategoryMaps.Any( x => x.HotelBestOfEnterpriseMaps.Any( hboem => hboem.FKDevice == deviceId ) )
								  },
								  e.EnterpriseCategoryMaps,
								  HCMImage =
									  e.EnterpriseCategoryMaps.FirstOrDefault()
										  .CategoryMap.HotelCategoryMaps.FirstOrDefault( x => x.FKDevice == deviceId )
										  .Image,
							  } );

				//var query = ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive )
				//			  let el = e.EnterpriseLocations.Where( el => el.IsActive && el.Coordinates.Distance( hotelPoint ) < radiusInMeters )
				//			  let ecm = e.EnterpriseCategoryMaps.Where( ecm2 => ecm2.IsActive )
				//			  let fecm = ecm.FirstOrDefault()
				//			  let cm = ecm.Where( ecm2 => ecm2.CategoryMap.IsActive ).Select( ecm2 => ecm2.CategoryMap )
				//			  let fcm = cm.FirstOrDefault()
				//			  let hcm = cm.SelectMany( cm2 => cm2.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive && hcm2.FKDevice == deviceId ) )
				//			  let fhcm = hcm.OrderBy( hcm2 => hcm2.Ordinal ).FirstOrDefault()
				//			  let hboem = ecm.SelectMany( ecm2 => ecm2.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive && hboem2.FKDevice == deviceId ).OrderBy( hboem2 => hboem2.Ordinal ) )
				//			  let hboelm = hboem.SelectMany( hboem2 => hboem2.HotelBestOfEnterpriseLocationMaps )
				//			  let blem = e.BlackListEnterpriseMaps.Where( blem2 => blem2.IsActive && blem2.FKDevice == deviceId )
				//			  let blelm = el.SelectMany( el2 => el2.BlackListEnterpriseLocationMaps.Where( blelm2 => blelm2.IsActive == true && blelm2.Hotel.Devices.Any( d => d.PKID == deviceId ) ) )
				//			  where el.Any()
				//			  select new
				//			  {
				//				  CMImage = fcm.Image,
				//				  ECMImage = fecm.Image,
				//				  Enterprise = new EnterpriseModel()
				//				  {
				//					  Blacklisted = blem.Any(),
				//					  EnterpriseLocations = ( from el2 in el
				//											  select new EnterpriseLocationModel()
				//											  {
				//												  BingId = el2.BingId,
				//												  Blacklisted = blelm.Select( blelm2 => blelm2.FKEnterpriseLocation ).Contains( el2.PKID ),
				//												  DistanceInMiles = ProxylessContext.CalculateDistance( el2.Latitude, el2.Longitude, hotelGeocode.Latitude.Value, hotelGeocode.Longitude.Value ),
				//												  Location = new LocationModel()
				//												  {
				//													  Address1 = el2.Address1,
				//													  Address2 = el2.Address2,
				//													  City = el2.City,
				//													  Country = new CountryModel
				//													  {
				//														  ISOCountryCode = el2.Country
				//													  },
				//													  Geocode = new GeocodeModel()
				//													  {
				//														  Latitude = el2.Latitude,
				//														  Longitude = el2.Longitude
				//													  },
				//													  PostalCode = el2.PostalCode,
				//													  State = new StateModel
				//													  {
				//														  ISOStateCode = el2.State,
				//													  }
				//												  },
				//												  Phone = el2.Phone,
				//												  PKID = el2.PKID,
				//												  Recommended = hboelm.Any( hboelm2 => hboelm2.FKEnterpriseLocation == el2.PKID )
				//											  } ),
				//					  Name = e.Name,
				//					  PKID = e.PKID,
				//					  Recommended = hboem.Any() && !hboelm.Any()
				//				  },
				//				  EnterpriseCategoryMaps = ecm,
				//				  HCMImage = fhcm.Image,
				//			  } );

				if( categoryId.HasValue )
					query = ( from q in query
							  where q.EnterpriseCategoryMaps.Any( ecm2 => ecm2.CategoryMap.FKChildCategory == categoryId.Value )
							  select q );

				if( !string.IsNullOrWhiteSpace( text ) )
				{
					var ft = ProxylessContext.GetEnterprisesByFreeText( text ).Select( gebft => gebft.Key );
					query = query.Where( x => ft.Contains( x.Enterprise.PKID ) );
				}

				switch( sortType )
				{
					case EnterpriseSearchSortTypes.ALAPHABETICAL:
						query = ( from q in query
								  orderby q.Enterprise.Name
								  select q );

						break;

					case EnterpriseSearchSortTypes.NEAREST:
						query = ( from q in query
								  orderby q.Enterprise.EnterpriseLocations.Min( el => el.DistanceInMeters ),
									  q.Enterprise.Name
								  select q );

						break;
				}

				var list = Rp.ExecuteAction( () => query.Take( 20 ) ).ToList();

				if( list != null )
					list.ForEach( e =>
					{
						if( e.Enterprise != null )
						{
							if( e.CMImage != null )
								e.Enterprise.ListItemImage = new ImageModel( e.CMImage );

							if( e.HCMImage != null )
								e.Enterprise.ListItemImage = new ImageModel( e.HCMImage );

							if( e.ECMImage != null )
								e.Enterprise.ListItemImage = new ImageModel( e.ECMImage );

							if( enterprises == null )
								enterprises = new List<EnterpriseModel>();

							enterprises.Add( e.Enterprise );
						}
					} );
			}

			return enterprises;
		}

		private void DeleteEnterpriseLocationRecommendation( int deviceId, int enterpriseId, int enterpriseLocationId,
			List<int> categoryIds )
		{
			if( categoryIds != null )
			{
				Rp.ExecuteAction( () =>
				{
					var hotelBestOfEnterpriseLocationMaps =
						( from hboelm in
							  ProxylessContext.HotelBestOfEnterpriseLocationMaps.Where( hboelm2 => hboelm2.HotelBestOfEnterpriseMap.IsActive )
						  let hboem = hboelm.HotelBestOfEnterpriseMap
						  where hboem.FKDevice == deviceId
								&& hboem.EnterpriseCategoryMap.FKEnterprise == enterpriseId
								&& hboelm.FKEnterpriseLocation == enterpriseLocationId
								&& categoryIds.Contains( hboem.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID )
						  select hboelm ).ToList();

					ProxylessContext.HotelBestOfEnterpriseLocationMaps.DeleteObjects( hotelBestOfEnterpriseLocationMaps );

					var hotelBestOfEnterpriseMap =
						( from hboem in ProxylessContext.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive )
						  where hboem.FKDevice == deviceId
								&& hboem.EnterpriseCategoryMap.FKEnterprise == enterpriseId
								&& !hboem.HotelBestOfEnterpriseLocationMaps.Any()
						  select hboem ).FirstOrDefault();

					if( hotelBestOfEnterpriseMap != null )
						ProxylessContext.HotelBestOfEnterpriseMaps.DeleteObject( hotelBestOfEnterpriseMap );
				} );
			}
		}

		private void DeleteEnterpriseRecommendation( int deviceId, int enterpriseId, List<int> categoryIds )
		{
			if( categoryIds != null )
			{
				Rp.ExecuteAction( () =>
				{
					var hotelBestOfEnterpriseMaps =
						( from hboem in ProxylessContext.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive )
						  where hboem.FKDevice == deviceId
								&& hboem.EnterpriseCategoryMap.FKEnterprise == enterpriseId
								&& categoryIds.Contains( hboem.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID )
						  select hboem ).ToList();

					ProxylessContext.HotelBestOfEnterpriseMaps.DeleteObjects( hotelBestOfEnterpriseMaps );
				} );
			}
		}

		private void InsertEnterpriseLocationRecommendation( int deviceId, int enterpriseId, int enterpriseLocationId,
			List<int> categoryIds )
		{
			if( categoryIds != null )
				Rp.ExecuteAction( () =>
				{
					foreach( var categoryId in categoryIds )
					{
						var hotelBestOfEnterprisLocationeMaps =
							( from hboelm in
								  ProxylessContext.HotelBestOfEnterpriseLocationMaps.Where( hboem => hboem.HotelBestOfEnterpriseMap.IsActive )
							  where hboelm.HotelBestOfEnterpriseMap.FKDevice == deviceId
									&& hboelm.HotelBestOfEnterpriseMap.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID == categoryId
							  select hboelm ).ToList();
						var hotelBestOfEnterpriseMap = InsertEnterpriseRecommendation( deviceId, enterpriseId, new List<int>
						{
							categoryId
						} );

						if( hotelBestOfEnterpriseMap != null )
						{
							var ordinal = hotelBestOfEnterprisLocationeMaps != null
										  && hotelBestOfEnterprisLocationeMaps.Count > 0
								? hotelBestOfEnterprisLocationeMaps.Max( hboem => hboem.Ordinal ) + 1
								: 1;

							var hotelBestOfEnterpriseLocationMap = new HotelBestOfEnterpriseLocationMap
							{
								FKEnterpriseLocation = enterpriseLocationId,
								HotelBestOfEnterpriseMap = hotelBestOfEnterpriseMap,
								Ordinal = ordinal,
								LastModifiedDateTime = DateTime.Now
							};

							ProxylessContext.HotelBestOfEnterpriseLocationMaps.Add( hotelBestOfEnterpriseLocationMap );
						}
					}
				} );
		}

		private HotelBestOfEnterpriseMap InsertEnterpriseRecommendation( int deviceId, int enterpriseId, List<int> categoryIds )
		{
			HotelBestOfEnterpriseMap hotelBestOfEnterpriseMap = null;

			if( categoryIds != null )
			{
				Rp.ExecuteAction( () =>
				{
					foreach( var categoryId in categoryIds )
					{
						var enterpriseCategoryMaps = ( from ecm in ProxylessContext.EnterpriseCategoryMaps
														   .Include( ecm => ecm.CategoryMap )
													   where ecm.FKEnterprise == enterpriseId
													   select ecm ).ToList();
						var enterpriseCategoryMap = ( from ecm in enterpriseCategoryMaps
													  where ecm.CategoryMap.FKChildCategory == categoryId
													  select ecm ).FirstOrDefault();

						if( enterpriseCategoryMap == null )
						{
							var firstEnterpriseCategoryMap = enterpriseCategoryMaps.FirstOrDefault( ecm2 => ecm2.FKImage != null );

							EnterpriseImageMap enterpriseImageMap = null;

							if( firstEnterpriseCategoryMap == null )
								enterpriseImageMap = ( from eim in ProxylessContext.EnterpriseImageMaps.Where( eim2 => eim2.IsActive )
													   orderby eim.Ordinal
													   select eim ).FirstOrDefault();

							var categoryMap = ( from cm in ProxylessContext.CategoryMaps.Where( cm2 => cm2.IsActive )
												where cm.FKParentCategory == null
													  && cm.FKChildCategory == categoryId
												select cm ).FirstOrDefault();

							if( categoryMap != null )
							{
								enterpriseCategoryMap = new EnterpriseCategoryMap
								{
									FKCategoryMap = categoryMap.PKID,
									FKImage = firstEnterpriseCategoryMap != null ? firstEnterpriseCategoryMap.FKImage : enterpriseImageMap.FKImage,
									FKEnterprise = enterpriseId,
								};

								ProxylessContext.EnterpriseCategoryMaps.Add( enterpriseCategoryMap );
							}
						}

						var hotelBestOfEnterpriseMaps =
							( from hboem in ProxylessContext.HotelBestOfEnterpriseMaps.Include( hboem => hboem.EnterpriseCategoryMap ).Where( hboem2 => hboem2.IsActive )
							  where hboem.FKDevice == deviceId
									&& hboem.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID == categoryId
							  select hboem ).ToList();

						if( enterpriseCategoryMap != null && hotelBestOfEnterpriseMaps != null &&
							hotelBestOfEnterpriseMaps.FirstOrDefault( hboem => hboem.EnterpriseCategoryMap.FKEnterprise == enterpriseId ) ==
							null )
						{
							var ordinal = hotelBestOfEnterpriseMaps != null && hotelBestOfEnterpriseMaps.Count > 0
								? hotelBestOfEnterpriseMaps.Max( hboem => hboem.Ordinal ) + 1
								: 1;

							hotelBestOfEnterpriseMap = new HotelBestOfEnterpriseMap
							{
								EnterpriseCategoryMap = enterpriseCategoryMap,
								FKDevice = deviceId,
								IsActive = true,
								IsLocalTransportation = false,
								Ordinal = ordinal,
							};

							ProxylessContext.HotelBestOfEnterpriseMaps.Add( hotelBestOfEnterpriseMap );
						}
					}
				} );
			}

			return hotelBestOfEnterpriseMap;
		}

		private void UpdateEnterprise( EnterpriseModel enterprise )
		{
			var culture = MvcApplication.GetCurrentCulture();
			var nativeLanguage = Monscierge.Utilities.Translation.GetLanguageFromCulture( culture );
			if( enterprise != null )
				Rp.ExecuteAction( () =>
				{
					var dbEnterprise = ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive )
										 where e.PKID == enterprise.PKID
										 select e ).FirstOrDefault();

					if( dbEnterprise != null )
					{
						if( dbEnterprise.DescriptionLanguage.Equals( nativeLanguage, StringComparison.InvariantCultureIgnoreCase ) )
							dbEnterprise.Description = enterprise.Description;
						else
							dbEnterprise.LocalizedDescription =
								Localization.SetLocalizedText( dbEnterprise.LocalizedDescription, nativeLanguage,
									enterprise.Description );
						dbEnterprise.FacebookUrl = enterprise.FacebookUrl;
						dbEnterprise.Name = enterprise.Name;
						dbEnterprise.TwitterUrl = enterprise.TwitterUrl;
						dbEnterprise.URL = enterprise.WebsiteUrl;
					}
				} );
		}

		private void UpdateEnterpriseLocation( EnterpriseLocationModel enterpriseLocation )
		{
			if( enterpriseLocation != null )
				Rp.ExecuteAction( () =>
				{
					var dbEnterpriseLocation = ( from el in ProxylessContext.EnterpriseLocations.Where( el2 => el2.IsActive )
												 where el.PKID == enterpriseLocation.PKID
												 select el ).FirstOrDefault();

					if( dbEnterpriseLocation != null )
					{
						dbEnterpriseLocation.Address1 = enterpriseLocation.Location != null ? enterpriseLocation.Location.Address1 : null;
						dbEnterpriseLocation.Address2 = enterpriseLocation.Location != null ? enterpriseLocation.Location.Address2 : null;
						dbEnterpriseLocation.City = enterpriseLocation.Location != null ? enterpriseLocation.Location.City : null;
						dbEnterpriseLocation.Country = enterpriseLocation.Location != null && enterpriseLocation.Location.Country != null
							? enterpriseLocation.Location.Country.ISOCountryCode
							: null;
						dbEnterpriseLocation.Phone = enterpriseLocation.Phone;
						dbEnterpriseLocation.PhoneISOCountryCode = enterpriseLocation.PhoneISOCountryCode;
						dbEnterpriseLocation.State = enterpriseLocation.Location != null && enterpriseLocation.Location.State != null
							? enterpriseLocation.Location.State.ISOStateCode
							: null;
						dbEnterpriseLocation.PostalCode = enterpriseLocation.Location != null
							? enterpriseLocation.Location.PostalCode
							: null;
					}
				} );
		}

		public void UpdateEnterprise( int deviceId, EnterpriseModel enterprise, EnterpriseLocationModel enterpriseLocation )
		{
			if( enterprise != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				UpdateEnterprise( enterprise );
				UpdateEnterpriseLocation( enterpriseLocation );

				Rp.ExecuteAction(
					() => ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );
			}
		}

		public void UpdateEnterprise( int deviceId, int enterpriseId, int? enterpriseLocationId, List<int> addedCategoryIds,
			List<int> removedCategoryIds )
		{
			if( addedCategoryIds != null || removedCategoryIds != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				if( !enterpriseLocationId.HasValue )
				{
					DeleteEnterpriseRecommendation( deviceId, enterpriseId, removedCategoryIds );
					InsertEnterpriseRecommendation( deviceId, enterpriseId, addedCategoryIds );
				}
				else
				{
					DeleteEnterpriseLocationRecommendation( deviceId, enterpriseId, enterpriseLocationId.Value, removedCategoryIds );
					InsertEnterpriseLocationRecommendation( deviceId, enterpriseId, enterpriseLocationId.Value, addedCategoryIds );
				}

				Rp.ExecuteAction(
					() => ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );
			}
		}
	}
}