using ConnectCMS.Models.Image;
using ConnectCMS.Models.Recommendation;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Collections.Generic;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class CategoryRepository : ChildRepository
	{
		public CategoryRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public CategoryRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		private void DeleteHotelCategories( int deviceId, List<int> removeCategoryIds )
		{
			if( removeCategoryIds != null )
				Rp.ExecuteAction( () =>
				{
					Context.HotelCategoryMaps.DeleteObjects( ( from hcm in Context.HotelCategoryMaps
															   where hcm.FKDevice == deviceId
																  && removeCategoryIds.Contains( hcm.CategoryMap.FKChildCategory )
															   select hcm ).ToList() );

					Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
				} );
		}

		public void DeleteHotelCategory( int deviceId, int categoryId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			DeleteHotelCategories( deviceId, new List<int>()
			{
				categoryId
			} );
		}

		private List<CategoryModel> GetCategories( int deviceId, bool onDevice )
		{
			List<CategoryModel> categories = null;

			var list = Rp.ExecuteAction( () => ( from cm in Context.CategoryMaps
												 let hcm = cm.HotelCategoryMaps.FirstOrDefault( hcm => hcm.FKDevice == deviceId )
												 where cm.IsActive
													 && cm.ChildCategory.IsActive
													 && cm.ParentCategory == null
													 && ( !onDevice || hcm != null )
												 select new
												 {
													 Category = new CategoryModel()
													 {
														 BackgroundColor = cm.Color,
														 Name = cm.ChildCategory.Name,
														 OnDevice = hcm != null,
														 Order = hcm != null ? hcm.Ordinal : 0,
														 PKID = cm.ChildCategory.PKID,
													 },
													 Image = hcm.Image,
													 DefaultImage = cm.Image,
												 } ).ToList() );

			if( list != null )
				list.ForEach( c =>
				{
					if( c.Category != null )
					{
						if( c.DefaultImage != null )
							c.Category.DefaultImage = new ImageModel( c.DefaultImage );

						c.Category.Image = c.Category.DefaultImage;

						if( c.Image != null )
							c.Category.Image = new ImageModel( c.Image );

						if( categories == null )
							categories = new List<CategoryModel>();

						categories.Add( c.Category );
					}
				} );

			return categories;
		}

		public List<CategoryModel> GetCategories( int deviceId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			var categories = GetCategories( deviceId, false );

			return categories;
		}

		public List<CategoryModel> GetCategoriesOnDevice( int deviceId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			var categories = GetCategories( deviceId, true );

			return categories;
		}

		public void UpdateCategories( int deviceId, List<int> addCategoryIds, List<int> removeCategoryIds )
		{
			if( addCategoryIds != null || removeCategoryIds != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				if( addCategoryIds != null )
					Rp.ExecuteAction( () =>
					{
						addCategoryIds.ForEach( id => Context.HotelCategoryMaps.Add( new HotelCategoryMap()
						{
							CategoryMap = Context.CategoryMaps.FirstOrDefault( cm => cm.FKChildCategory == id && cm.FKParentCategory == null ),
							FKDevice = deviceId
						} ) );

						Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
					} );

				if( removeCategoryIds != null )
					DeleteHotelCategories( deviceId, removeCategoryIds );
			}
		}

		public void UpdateCategories( int deviceId, List<CategoryModel> categories )
		{
			if( categories != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				Rp.ExecuteAction( () =>
				{
					HotelCategoryMap hotelCategoryMap;

					foreach( var category in categories )
					{
						hotelCategoryMap = ( from hcm in Context.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive == null || hcm2.IsActive )
											 let cm = hcm.CategoryMap
											 let c = cm.ChildCategory
											 where hcm.FKDevice == deviceId
											 && c != null
											 && c.PKID == category.PKID
											 select hcm ).FirstOrDefault();

						if( hotelCategoryMap != null )
							hotelCategoryMap.Ordinal = category.Order;
					}

					Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
				} );
			}
		}

		public void UpdateCategory( int deviceId, CategoryModel category )
		{
			if( category != null )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

				Rp.ExecuteAction( () =>
				{
					CategoryMap categoryMap = ( from cm in Context.CategoryMaps.Where( cm2 => cm2.IsActive )
												where cm.FKChildCategory == category.PKID
												&& cm.FKParentCategory == null
												select cm ).FirstOrDefault();

					if( categoryMap != null )
					{
						HotelCategoryMap hotelCategoryMap = ( from hcm in Context.HotelCategoryMaps.Where( hcm2 => hcm2.IsActive )
															  where hcm.FKDevice == deviceId
															  && hcm.FKCategoryMap == categoryMap.PKID
															  select hcm ).FirstOrDefault();

						if( hotelCategoryMap != null )
						{
							hotelCategoryMap.FKImage = category.Image.PKID;

							Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
						}
					}
				} );
			}
		}
	}
}