using ConnectCMS.Models.ContactUser;
using ConnectCMS.Models.Enterprise;
using ConnectCMS.Models.Image;
using ConnectCMS.Models.Location;
using ConnectCMS.Models.Recommendation;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Utils;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class RecommendationRepository : ChildRepository
	{
		public RecommendationRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public RecommendationRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public void DeleteEnterpriseBlacklist( int deviceId, int enterpriseId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			Rp.ExecuteAction( () =>
			{
				Context.BlackListEnterpriseMaps.DeleteObject( ( from blem in Context.BlackListEnterpriseMaps
																where blem.FKDevice == deviceId
																   && blem.FKEnterprise == enterpriseId
																select blem ).FirstOrDefault() );

				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		public void DeleteEnterpriseLocationBlacklist( int deviceId, int enterpriseLocationId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			Rp.ExecuteAction( () =>
			{
				Context.BlackListEnterpriseLocationMaps.DeleteObject( ( from blelm in Context.BlackListEnterpriseLocationMaps
																		where blelm.Hotel.Devices.Select( d => d.PKID ).Contains( deviceId )
																		   && blelm.FKEnterpriseLocation == enterpriseLocationId
																		select blelm ).FirstOrDefault() );

				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		public void DeleteEnterpriseLocationRecommend( int deviceId, int enterpriseId, int enterpriseLocationId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			Rp.ExecuteAction( () =>
			{
				Context.HotelBestOfEnterpriseLocationMaps.DeleteObjects( ( from hboelm in Context.HotelBestOfEnterpriseLocationMaps
																		   let hboem = hboelm.HotelBestOfEnterpriseMap
																		   where hboem.FKDevice == deviceId
																			  && hboem.EnterpriseCategoryMap.FKEnterprise == enterpriseId
																			  && hboelm.FKEnterpriseLocation == enterpriseLocationId
																		   select hboelm ).ToList() );

				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		public void DeleteEnterpriseRecommend( int deviceId, int enterpriseId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			Rp.ExecuteAction( () =>
			{
				Context.HotelBestOfEnterpriseMaps.DeleteObjects( ( from hboem in Context.HotelBestOfEnterpriseMaps
																   where hboem.FKDevice == deviceId
																	   && hboem.EnterpriseCategoryMap.FKEnterprise == enterpriseId
																   select hboem ).ToList() );

				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		public List<EnterpriseModel> GetEnterprisesWithTipsForHotel( int deviceId, float? hotelLatitude, float? hotelLongitude, float radiusInMeters )
		{
			List<EnterpriseModel> enterprises = null;

			DbGeography hotelPoint = null;

			if( hotelLatitude.HasValue && hotelLongitude.HasValue )
				hotelPoint = GeographyUtils.GetPointFromLatitudeAndLongitude( hotelLatitude.Value, hotelLongitude.Value );

			if( hotelPoint != null )
			{
				var list = Rp.ExecuteAction( () => ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive )
													 let el = e.EnterpriseLocations.Where( el2 => el2.IsActive )
													 let ecm = e.EnterpriseCategoryMaps
													 let fecm = ecm.FirstOrDefault()
													 let cm = ecm.Where( ecm2 => ecm2.CategoryMap.IsActive ).Select( ecm2 => ecm2.CategoryMap )
													 let fcm = cm.FirstOrDefault()
													 let hcm = cm.SelectMany( cm2 => cm2.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive && hcm2.FKDevice == deviceId ) )
													 let fhcm = hcm.OrderBy( hcm2 => hcm2.Ordinal ).FirstOrDefault()
													 let eit = e.InsiderTips.Where( it2 => it2.IsActive && !it2.FKEnterpriseLocation.HasValue )
													 let elit = el.SelectMany( el2 => el2.InsiderTips.Where( it2 => it2.IsActive ) )
													 where el.Any( el2 => ( !elit.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
															 || elit.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID ) )
														 && hcm.Any()
														 && ( eit.Any() || elit.Any() )
													 select new
													 {
														 CMImage = fcm.Image,
														 ECMImage = fecm.Image,
														 Enterprise = new EnterpriseModel
														 {
															 EnterpriseLocations = ( from el2 in el
																					 let d = el2.Coordinates.Distance( hotelPoint )
																					 where el2.Coordinates.Distance( hotelPoint ) < radiusInMeters
																						 || elit.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID )
																					 orderby d
																					 select new EnterpriseLocationModel
																					 {
																						 DistanceInMeters = d,
																						 Location = new LocationModel
																						 {
																							 Address1 = el2.Address1,
																							 Address2 = el2.Address2,
																							 City = el2.City,
																							 ISOCountryCode = el2.Country,
																							 Latitude = el2.Latitude,
																							 Longitude = el2.Longitude,
																							 PostalCode = el2.PostalCode,
																							 ISOStateCode = el2.State,
																						 },
																						 Phone = el2.Phone,
																						 PKID = el2.PKID,
																						 Tips = ( from elit2 in elit
																								  let c = elit2.ContactUser
																								  where el2.PKID == elit2.FKEnterpriseLocation
																								  select new TipModel
																								  {
																									  ContactUser = new ContactUserModel
																									  {
																										  Name = c.ContactUserName,
																										  PKID = c.PKID
																									  },
																									  LastModified = elit2.TipDateTime,
																									  LocalTip = elit2.Tip,
																									  LocalTipLanguage = elit2.TipLanguage,
																									  LocalTipXml = elit2.LocalizedTip,
																									  PKID = elit2.PKID
																								  } )
																					 } ),
															 Name = e.Name,
															 PKID = e.PKID,
															 Tips = ( from eit2 in eit
																	  let c = eit2.ContactUser
																	  select new TipModel
																	  {
																		  ContactUser = new ContactUserModel
																		  {
																			  Name = c.ContactUserName,
																			  PKID = c.PKID
																		  },
																		  LastModified = eit2.TipDateTime,
																		  LocalTip = eit2.Tip,
																		  LocalTipLanguage = eit2.TipLanguage,
																		  LocalTipXml = eit2.LocalizedTip,
																		  PKID = eit2.PKID
																	  } )
														 },
														 HCMImage = fhcm.Image,
													 } ) ).Take( 10 ).ToList();

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

						e.Enterprise.EnterpriseLocations.ForEach( el => el.Tips.ForEach( t => t.Tip = Localization.GetLocalizedText( t.LocalTip, t.LocalTipXml, t.LocalTipLanguage ) ) );
						e.Enterprise.Tips.ForEach(
							t => t.Tip = Localization.GetLocalizedText( t.LocalTip, t.LocalTipXml, t.LocalTipLanguage ) );

						if( enterprises == null )
							enterprises = new List<EnterpriseModel>();

						enterprises.Add( e.Enterprise );
					}
				} );
			}

			return enterprises;
		}

		public List<EnterpriseModel> GetRecommendedEnterprisesOnDevice( int deviceId, float? hotelLatitude, float? hotelLongitude, float radiusInMeters )
		{
			List<EnterpriseModel> enterprises = null;

			var hotelPoint = GeographyUtils.GetPointFromLatitudeAndLongitude( hotelLatitude, hotelLongitude );

			if( hotelPoint != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				enterprises = Rp.ExecuteAction( () => ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive )
														let el = e.EnterpriseLocations.Where( el2 => el2.IsActive && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
														let ecm = e.EnterpriseCategoryMaps
														let fecm = ecm.FirstOrDefault()
														let cm = ecm.Where( ecm2 => ecm2.CategoryMap.IsActive ).Select( ecm2 => ecm2.CategoryMap )
														let fcm = cm.FirstOrDefault()
														let hcm = cm.SelectMany( cm2 => cm2.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive && hcm2.FKDevice == deviceId ) )
														let fhcm = hcm.OrderBy( hcm2 => hcm2.Ordinal ).FirstOrDefault()
														let hboem = ecm.SelectMany( ecm2 => ecm2.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive && hboem2.FKDevice == deviceId ).OrderBy( hboem2 => hboem2.Ordinal ) )
														let hboelm = hboem.SelectMany( hboem2 => hboem2.HotelBestOfEnterpriseLocationMaps )
														where el.Any( el2 => ( !hboelm.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
																|| hboelm.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID ) )
															&& hcm.Any()
															&& ( hboem.Any() || hboelm.Any() )
														orderby hboem.Min( hboem2 => hboem2.Ordinal ),
														   hboelm.Min( hboelm2 => hboelm2.Ordinal )
														select new EnterpriseModel
														{
															EnterpriseLocations = ( from el2 in el
																					where ( !hboelm.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
																					 || hboelm.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID )
																					select new EnterpriseLocationModel
																					{
																						DistanceInMeters = el2.Coordinates.Distance( hotelPoint ),
																						Location = new LocationModel
																						{
																							Address1 = el2.Address1,
																							Address2 = el2.Address2,
																							City = el2.City,
																							ISOCountryCode = el2.Country,
																							Latitude = el2.Latitude,
																							Longitude = el2.Longitude,
																							PostalCode = el2.PostalCode,
																							ISOStateCode = el2.State
																						},
																						Phone = el2.Phone,
																						PKID = el2.PKID,
																						Recommended = hboelm.Any( hboelm2 => hboelm2.FKEnterpriseLocation == el2.PKID ),
																						RecommendedCategories = ( from hboelm2 in hboelm
																												  select new OrderModel
																												  {
																													  Key = hboelm2.HotelBestOfEnterpriseMap.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID,
																													  Order = hboelm2.Ordinal
																												  } )
																					} ),
															Name = e.Name,
															PKID = e.PKID,
															Recommended = hboem.Any() && !hboelm.Any(),
															RecommendedCategories = ( from hboem2 in hboem
																					  select new OrderModel
																					  {
																						  Key = hboem2.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID,
																						  Order = hboem2.Ordinal
																					  } )
														} ) ).ToList();
			}

			return enterprises;
		}

		public List<EnterpriseModel> GetSearchBlacklistedEnterprisesOnDevice( int deviceId, float? hotelLatitude, float? hotelLongitude, float radiusInMeters, int? categoryId, string text )
		{
			List<EnterpriseModel> enterprises = null;

			var hotelPoint = GeographyUtils.GetPointFromLatitudeAndLongitude( hotelLatitude, hotelLongitude );

			if( hotelPoint != null )
			{
				var hotel = RootRepository.HotelRepository.GetHotelFromDevice( deviceId );
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				if( text != null )
					text = text.Trim();

				var query =
					( from e in
						  ProxylessContext.Enterprises
							  .Include( e => e.EnterpriseLocations )
							  .Include( e => e.EnterpriseCategoryMaps.Select( ecm => ecm.Image ) )
							  .Include( e => e.EnterpriseCategoryMaps.Select( ecm => ecm.CategoryMap.Image ) )
							  .Include(
								  e =>
									  e.EnterpriseCategoryMaps.Select(
										  ecm => ecm.HotelBestOfEnterpriseMaps.Select( hboem => hboem.HotelBestOfEnterpriseLocationMaps ) ) )
							  .Where(
								  e =>
									  e.IsActive &&
									  ( e.BlackListEnterpriseMaps.Any( blem => blem.IsActive && blem.FKDevice == deviceId ) ||
									   e.EnterpriseLocations.Any( el => el.BlackListEnterpriseLocationMaps.Any( blemlm => blemlm.IsActive && blemlm.FKHotel == hotel.PKID ) ) ) )
					  select new
					  {
						  BlackListEnterpriseMaps = e.BlackListEnterpriseMaps.Where( blem => blem.IsActive ),
						  BlackListEnterpriseLocationMaps =
							  e.EnterpriseLocations.SelectMany( el => el.BlackListEnterpriseLocationMaps.Where( blelm => blelm.IsActive ) ),
						  CMImage = e.EnterpriseCategoryMaps
							  .Where(
								  ecm => ecm.CategoryMap != null && ecm.CategoryMap.IsActive && ecm.CategoryMap.Image != null &&
									  ecm.CategoryMap.Image.IsActive )
							  .Select( ecm => ecm.CategoryMap.Image )
							  .FirstOrDefault( i => i != null && i.IsActive ),
						  ECMImage = e.EnterpriseCategoryMaps
							  .Where( ecm => ecm.CategoryMap != null && ecm.CategoryMap.Image != null )
							  .Select( ecm => ecm.Image )
							  .FirstOrDefault( i => i != null && i.IsActive ),
						  HCMImage = e.EnterpriseCategoryMaps
							  .Where( ecm => ecm.CategoryMap != null )
							  .SelectMany(
								  ecm =>
									  ecm.CategoryMap.HotelCategoryMaps.Where( hcm => hcm != null && hcm.FKDevice == deviceId && hcm.Image != null )
										  .Select( hcm => hcm.Image ) ).FirstOrDefault(),
						  HotelCategoryMaps =
							  e.EnterpriseCategoryMaps.SelectMany(
								  ecm => ecm.CategoryMap.HotelCategoryMaps.Where( hcm => hcm.IsActive && hcm.FKDevice == deviceId ) ),
						  Enterprise = new EnterpriseModel
						  {
							  Blacklisted = e.BlackListEnterpriseMaps.Any( blem => blem.IsActive && blem.FKDevice == deviceId ),
							  Recommended =
								  e.EnterpriseCategoryMaps.Any( ecm => ecm.HotelBestOfEnterpriseMaps.Any( hboem => hboem.FKDevice == deviceId ) ),
							  Name = e.Name,
							  PKID = e.PKID,
							  EnterpriseLocations = e.EnterpriseLocations
								  .Where(
									  el =>
										  el.IsActive && el.Coordinates.Distance( hotelPoint ) < radiusInMeters &&
										  ( el.Enterprise.BlackListEnterpriseMaps.Any( blem => blem.IsActive ) ||
										   el.BlackListEnterpriseLocationMaps.Any( blelm => blelm.IsActive ) ) )
								  .Select( el => new EnterpriseLocationModel
								  {
									  Blacklisted = el.BlackListEnterpriseLocationMaps.Any( blelm => blelm.FKHotel == hotel.PKID ),
									  Recommended =
										  el.HotelBestOfEnterpriseLocationMaps.Any( hboelm => hboelm.HotelBestOfEnterpriseMap.FKDevice == deviceId ),
									  DistanceInMeters = el.Coordinates.Distance( hotelPoint ),
									  Phone = el.Phone,
									  PKID = el.PKID,
									  Location = new LocationModel
									  {
										  Address1 = el.Address1,
										  Address2 = el.Address2,
										  City = el.City,
										  ISOCountryCode = el.Country,
										  ISOStateCode = el.State,
										  Latitude = el.Latitude,
										  Longitude = el.Longitude,
										  PostalCode = el.PostalCode,
									  }
								  } )
						  },
					  } );

				//var query = ( from e in ProxylessContext.Enterprises.Where( e2 => e2.IsActive )
				//			  let el = e.EnterpriseLocations.Where( el2 => el2.IsActive )
				//			  let ecm = e.EnterpriseCategoryMaps.Where( ecm2 => ecm2.IsActive )
				//			  let fecm = ecm.FirstOrDefault()
				//			  let cm = ecm.Where( ecm2 => ecm2.CategoryMap.IsActive ).Select( ecm2 => ecm2.CategoryMap )
				//			  let fcm = cm.FirstOrDefault()
				//			  let hcm = cm.SelectMany( cm2 => cm2.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive && hcm2.FKDevice == deviceId ) )
				//			  let fhcm = hcm.OrderBy( hcm2 => hcm2.Ordinal ).FirstOrDefault()
				//			  let hboem = ecm.SelectMany( ecm2 => ecm2.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive && hboem2.FKDevice == deviceId ).OrderBy( hboem2 => hboem2.Ordinal ) )
				//			  let hboelm = hboem.SelectMany( hboem2 => hboem2.HotelBestOfEnterpriseLocationMaps )
				//			  let blem = e.BlackListEnterpriseMaps.Where( blem2 => blem2.IsActive && blem2.FKDevice == deviceId )
				//			  let blelm = el.SelectMany( el2 => el2.BlackListEnterpriseLocationMaps.Where( blelm2 => blelm2.IsActive && blelm2.Hotel.Devices.Any( d => d.PKID == deviceId ) ) )
				//			  where el.Any( el2 => ( !blelm.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
				//					|| blelm.Select( blelm2 => blelm2.FKEnterpriseLocation ).Contains( el2.PKID ) )
				//				&& hcm.Any()
				//				&& ( blem.Any() || blelm.Any() )
				//			  select new
				//			  {
				//				  BlackListEnterpriseMaps = blem,
				//				  BlackListEnterpriseLocationMaps = blelm,
				//				  CMImage = fcm.Image,
				//				  ECMImage = fecm.Image,
				//				  Enterprise = new EnterpriseModel
				//				  {
				//					  Blacklisted = blem.Any(),
				//					  EnterpriseLocations = ( from el2 in el
				//											  where ( !blelm.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
				//												  || blelm.Select( blelm2 => blelm2.FKEnterpriseLocation ).Contains( el2.PKID )
				//											  select new EnterpriseLocationModel
				//											  {
				//												  Blacklisted = blelm.Select( blelm2 => blelm2.FKEnterpriseLocation ).Contains( el2.PKID ),
				//												  DistanceInMiles = ProxylessContext.CalculateDistance( el2.Latitude, el2.Longitude, hotelGeocode.Latitude.Value, hotelGeocode.Longitude.Value ),
				//												  Location = new LocationModel
				//												  {
				//													  Address1 = el2.Address1,
				//													  Address2 = el2.Address2,
				//													  City = el2.City,
				//													  ISOCountryCode = el2.Country,
				//													  Geocode = new GeocodeModel
				//													  {
				//														  Latitude = el2.Latitude,
				//														  Longitude = el2.Longitude
				//													  },
				//													  PostalCode = el2.PostalCode,
				//													  ISOStateCode = el2.State,
				//												  },
				//												  Phone = el2.Phone,
				//												  PKID = el2.PKID,
				//												  Recommended = hboelm.Any( hboelm2 => hboelm2.FKEnterpriseLocation == el2.PKID )
				//											  } ),
				//					  Name = e.Name,
				//					  PKID = e.PKID,
				//					  Recommended = hboem.Any() && !hboelm.Any()
				//				  },
				//				  HCMImage = fhcm.Image,
				//				  HotelCategoryMaps = hcm
				//			  } );

				if( categoryId.HasValue )
					query = ( from q in query
							  where q.HotelCategoryMaps.Any( hcm => hcm.CategoryMap.FKChildCategory == categoryId.Value )
							  select q );

				if( !string.IsNullOrWhiteSpace( text ) )
					query = ( from q in query
							  let gebft = ProxylessContext.GetEnterprisesByFreeText( text ).Select( gebft => gebft.Key )
							  where gebft.Contains( q.Enterprise.PKID )
							  select q );

				var list = query.Take( 20 ).ToList();

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

		public List<EnterpriseModel> GetSearchRecommendedEnterprisesOnDevice( int deviceId, float? hotelLatitude, float? hotelLongitude, float radiusInMeters, int? categoryId, string text )
		{
			List<EnterpriseModel> enterprises = null;

			var hotelPoint = GeographyUtils.GetPointFromLatitudeAndLongitude( hotelLatitude, hotelLongitude );

			if( hotelPoint != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				if( text != null )
					text = text.Trim();

				var query = ( from hboem in Context.HotelBestOfEnterpriseMaps
					 .Include( x => x.EnterpriseCategoryMap.CategoryMap )
					 .Include( x => x.EnterpriseCategoryMap.CategoryMap.HotelCategoryMaps )
					 .Include( x => x.EnterpriseCategoryMap.Enterprise )
					 .Include( x => x.EnterpriseCategoryMap.Enterprise.EnterpriseLocations )
					 .Include( x => x.EnterpriseCategoryMap.Enterprise.EnterpriseImageMaps.Select( eim => eim.Image ) )
					 .Include( x => x.HotelBestOfEnterpriseLocationMaps.Select( hboelm => hboelm.EnterpriseLocation ) )
							  where
							  hboem.IsActive &&
							  hboem.FKDevice == deviceId &&
							  hboem.EnterpriseCategoryMap.Enterprise.IsActive &&
							  hboem.EnterpriseCategoryMap.Enterprise.EnterpriseLocations.Any( x => x.IsActive ) &&
							  hboem.EnterpriseCategoryMap.CategoryMap.IsActive &&
							  ( !hboem.HotelBestOfEnterpriseLocationMaps.Any() || hboem.HotelBestOfEnterpriseLocationMaps.Any( x => x.EnterpriseLocation.IsActive ) )
							  select new
							  {
								  CMImage = hboem.EnterpriseCategoryMap.CategoryMap.Image,
								  ECMImage = hboem.EnterpriseCategoryMap.Image,
								  Enterprise = new EnterpriseModel
								  {
									  Blacklisted = false,
									  Name = hboem.EnterpriseCategoryMap.Enterprise.Name,
									  PKID = hboem.EnterpriseCategoryMap.Enterprise.PKID,
									  Recommended = true,
									  EnterpriseLocations = hboem.EnterpriseCategoryMap.Enterprise.EnterpriseLocations.Where( x => x.IsActive ).Select( x => new EnterpriseLocationModel
									  {
										  Blacklisted = false,
										  DistanceInMeters = x.Coordinates.Distance( hotelPoint ),
										  Location = new LocationModel
										  {
											  Address1 = x.Address1,
											  Address2 = x.Address2,
											  City = x.City,
											  ISOCountryCode = x.Country,
											  Latitude = x.Latitude,
											  Longitude = x.Longitude,
											  PostalCode = x.PostalCode,
											  ISOStateCode = x.State,
										  },
										  Phone = x.Phone,
										  PKID = x.PKID,
										  Recommended = hboem.HotelBestOfEnterpriseLocationMaps.Any( hboelm2 => hboelm2.FKEnterpriseLocation == x.PKID )
									  } )
								  },
								  HCMImage = hboem.EnterpriseCategoryMap.CategoryMap.HotelCategoryMaps.FirstOrDefault( x => x.FKDevice == deviceId ).Image,
								  HotelCategoryMaps = hboem.EnterpriseCategoryMap.CategoryMap.HotelCategoryMaps.Where( x => x.FKDevice == deviceId )
							  } );

				//var query = ( from e in Context.Enterprises.Where( e2 => e2.IsActive )
				//			  let el = e.EnterpriseLocations.Where( el2 => el2.IsActive )
				//			  let ecm = e.EnterpriseCategoryMaps.Where( ecm2 => ecm2.IsActive )
				//			  let fecm = ecm.FirstOrDefault()
				//			  let cm = ecm.Where( ecm2 => ecm2.CategoryMap.IsActive ).Select( ecm2 => ecm2.CategoryMap )
				//			  let fcm = cm.FirstOrDefault()
				//			  let hcm = cm.SelectMany( cm2 => cm2.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive && hcm2.FKDevice == deviceId ) )
				//			  let fhcm = hcm.OrderBy( hcm2 => hcm2.Ordinal ).FirstOrDefault()
				//			  let hboem = ecm.SelectMany( ecm2 => ecm2.HotelBestOfEnterpriseMaps.Where( hboem2 => hboem2.IsActive && hboem2.FKDevice == deviceId ).OrderBy( hboem2 => hboem2.Ordinal ) )
				//			  let hboelm = hboem.SelectMany( hboem2 => hboem2.HotelBestOfEnterpriseLocationMaps )
				//			  let blem = e.BlackListEnterpriseMaps.Where( blem2 => blem2.IsActive && blem2.FKDevice == deviceId )
				//			  let blelm = el.SelectMany( el2 => el2.BlackListEnterpriseLocationMaps.Where( blelm2 => blelm2.IsActive && blelm2.Hotel.Devices.Any( d => d.PKID == deviceId ) ) )
				//			  where el.Any( el2 => ( !hboelm.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
				//					  || hboelm.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID ) )
				//				  && hcm.Any()
				//				  && ( hboem.Any() || hboelm.Any() )
				//			  select new
				//			  {
				//				  CMImage = fcm.Image,
				//				  ECMImage = fecm.Image,
				//				  Enterprise = new EnterpriseModel
				//				  {
				//					  Blacklisted = blem.Any(),
				//					  EnterpriseLocations = ( from el2 in el
				//											  where ( !hboelm.Any() && el2.Coordinates.Distance( hotelPoint ) < radiusInMeters )
				//												  || hboelm.Select( hboelm2 => hboelm2.FKEnterpriseLocation ).Contains( el2.PKID )
				//											  select new EnterpriseLocationModel
				//											  {
				//												  Blacklisted = blelm.Select( blelm2 => blelm2.FKEnterpriseLocation ).Contains( el2.PKID ),
				//												  DistanceInMiles = Context.CalculateDistance( el2.Latitude, el2.Longitude, hotelGeocode.Latitude.Value, hotelGeocode.Longitude.Value ),
				//												  Location = new LocationModel
				//												  {
				//													  Address1 = el2.Address1,
				//													  Address2 = el2.Address2,
				//													  City = el2.City,
				//													  Country = new CountryModel
				//													  {
				//														  ISOCountryCode = el2.Country
				//													  },
				//													  Geocode = new GeocodeModel
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
				//				  HCMImage = fhcm.Image,
				//				  HotelCategoryMaps = hcm
				//			  } );

				if( categoryId.HasValue )
					query = ( from q in query
							  where q.HotelCategoryMaps.Any( hcm => hcm.CategoryMap.FKChildCategory == categoryId.Value )
							  select q );

				if( !string.IsNullOrWhiteSpace( text ) )
					query = ( from q in query
							  let gebft = Context.GetEnterprisesByFreeText( text ).Select( gebft => gebft.Key )
							  where gebft.Contains( q.Enterprise.PKID )
							  select q );

				var list = query.Take( 20 ).ToList();

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

		public void InsertEnterpriseBlacklist( int deviceId, int enterpriseId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			DeleteEnterpriseRecommend( deviceId, enterpriseId );

			Rp.ExecuteAction( () =>
			{
				var hotelId = ( from d in Context.Devices.Where( d2 => d2.DeviceDetail.IsActive )
								where d.PKID == deviceId
								select d.FKHotel ).FirstOrDefault();

				var blacklistEnterpriseMap = new BlackListEnterpriseMap
				{
					FKDevice = deviceId,
					FKEnterprise = enterpriseId,
					FKHotel = hotelId,
					IsActive = true
				};

				Context.BlackListEnterpriseMaps.AddObject( blacklistEnterpriseMap );
				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		public void InsertEnterpriseLocationBlacklist( int deviceId, int enterpriseLocationId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			var enterpriseId = Rp.ExecuteAction( () => ( from el in Context.EnterpriseLocations
														 where el.PKID == enterpriseLocationId
														 select el.FKEnterprise ) ).FirstOrDefault();

			DeleteEnterpriseLocationRecommend( deviceId, enterpriseId, enterpriseLocationId );

			Rp.ExecuteAction( () =>
			{
				var hotelId = ( from d in Context.Devices.Where( d2 => d2.DeviceDetail.IsActive )
								where d.PKID == deviceId
								select d.FKHotel ).FirstOrDefault();

				var blacklistEnterpriseLocationMap = new BlackListEnterpriseLocationMap
				{
					FKEnterpriseLocation = enterpriseLocationId,
					FKHotel = hotelId
				};

				Context.BlackListEnterpriseLocationMaps.AddObject( blacklistEnterpriseLocationMap );
				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		public void UpdateRecommendationReorder( int deviceId, List<EnterpriseModel> enterprises )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			if( enterprises != null )
				Rp.ExecuteAction( () =>
				{
					foreach( var enterprise in enterprises )
					{
						if( enterprise.RecommendedCategories != null )
							foreach( var recommendedCategory in enterprise.RecommendedCategories )
							{
								var hotelBestOfEnterpriseMap = Context.HotelBestOfEnterpriseMaps.FirstOrDefault( hboem => hboem.FKDevice == deviceId
																																				&& hboem.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID == recommendedCategory.Key
																																				&& hboem.EnterpriseCategoryMap.FKEnterprise == enterprise.PKID );

								if( hotelBestOfEnterpriseMap != null )
									hotelBestOfEnterpriseMap.Ordinal = recommendedCategory.Order;
							}

						if( enterprise.EnterpriseLocations != null )
							foreach( var enterpriseLocation in enterprise.EnterpriseLocations )
								if( enterpriseLocation.RecommendedCategories != null )
									foreach( var recommendedCategory in enterpriseLocation.RecommendedCategories )
									{
										var hotelBestOfEnterpriseLocationMap = Context.HotelBestOfEnterpriseLocationMaps.FirstOrDefault( hboelm => hboelm.HotelBestOfEnterpriseMap.FKDevice == deviceId
																																												 && hboelm.HotelBestOfEnterpriseMap.EnterpriseCategoryMap.CategoryMap.ChildCategory.PKID == recommendedCategory.Key
																																												 && hboelm.HotelBestOfEnterpriseMap.EnterpriseCategoryMap.FKEnterprise == enterprise.PKID
																																												 && hboelm.FKEnterpriseLocation == enterpriseLocation.PKID );

										if( hotelBestOfEnterpriseLocationMap != null )
											hotelBestOfEnterpriseLocationMap.Ordinal = recommendedCategory.Order;
									}
					}

					Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
				} );
		}
	}
}