using ConnectCMS.Models.Recommendation;
using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class CategoryController : ControllerBase
	{
		// GET: /Category/CategoryEdit
		public ActionResult Edit()
		{
			return PartialView( "_Edit" );
		}

		// GET: /Category/CategoryManage
		public ActionResult Manage()
		{
			return PartialView( "_Manage" );
		}

		// GET: /Category/CategoryReorder
		public ActionResult Reorder()
		{
			return PartialView( "_Reorder" );
		}

		// POST: /Category/DeleteCategory
		[HttpPost]
		public JsonNetResult DeleteCategory( int deviceId, int categoryId )
		{
			ConnectCmsRepository.CategoryRepository.DeleteHotelCategory( deviceId, categoryId );

			return JsonNet( true );
		}

		// POST: /Category/GetCategories
		[HttpPost]
		public JsonNetResult GetCategories( int deviceId )
		{
			var categories = ConnectCmsRepository.CategoryRepository.GetCategories( deviceId );

			return JsonNet( categories ?? new List<CategoryModel>() );
		}

		// POST: /Category/GetCategoriesOnDevice
		[HttpPost]
		public JsonNetResult GetCategoriesOnDevice( int deviceId )
		{
			var categories = ConnectCmsRepository.CategoryRepository.GetCategoriesOnDevice( deviceId );

			return JsonNet( categories ?? new List<CategoryModel>() );
		}

		// POST: /Category/UpdateCategoryEdit
		[HttpPost]
		public JsonNetResult UpdateCategoryEdit( int deviceId, CategoryModel category )
		{
			ConnectCmsRepository.CategoryRepository.UpdateCategory( deviceId, category );

			return JsonNet( true );
		}

		// POST: /Category/UpdateCategoriesManage
		[HttpPost]
		public JsonNetResult UpdateCategoriesManage( int deviceId, List<int> addCategoryIds, List<int> removeCategoryIds )
		{
			ConnectCmsRepository.CategoryRepository.UpdateCategories( deviceId, addCategoryIds, removeCategoryIds );

			return JsonNet( true );
		}

		// POST: /Category/UpdateCategories
		[HttpPost]
		public JsonNetResult UpdateCategoriesReorder( int deviceId, List<CategoryModel> categories )
		{
			ConnectCmsRepository.CategoryRepository.UpdateCategories( deviceId, categories );

			return JsonNet( true );
		}
	}
}