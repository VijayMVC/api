using ConnectCMS.Models.Enterprise;
using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class EnterpriseController : ControllerBase
	{
		// GET: /Enterprise/Edit
		public ActionResult Edit()
		{
			return PartialView( "_Edit" );
		}

		// GET: /Enterprise/Location
		public ActionResult Location()
		{
			return PartialView( "_Location" );
		}

		//POST: /Enterprise/GetEnterpriseForEdit
		[HttpPost]
		public JsonNetResult GetEnterpriseForEdit( int deviceId, int enterpriseId, int? enterpriseLocationId )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			EnterpriseModel enterprise = null;

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					enterprise = ConnectCmsRepository.EnterpriseRepository.GetEnterpriseForEdit( deviceId, enterpriseId, enterpriseLocationId, location.Latitude, location.Longitude, hotel.RadiusInMeters );
			}

			return JsonNet( enterprise ?? new EnterpriseModel() );
		}

		//POST: /Enterprise/GetEnterpriseRecommendationForEdit
		[HttpPost]
		public JsonNetResult GetEnterpriseRecommendationForEdit( int deviceId, int enterpriseId, int? enterpriseLocationId )
		{
			EnterpriseModel enterprise = ConnectCmsRepository.EnterpriseRepository.GetEnterpriseRecommendationForEdit( deviceId, enterpriseId, enterpriseLocationId );

			return JsonNet( enterprise ?? new EnterpriseModel() );
		}

		//POST: /Enterprise/GetEnterpriseTipForEdit
		[HttpPost]
		public JsonNetResult GetEnterpriseTipForEdit( int deviceId, int enterpriseId, int? enterpriseLocationId )
		{
			EnterpriseModel enterprise = ConnectCmsRepository.EnterpriseRepository.GetEnterpriseTipForEdit( deviceId, enterpriseId, enterpriseLocationId );

			return JsonNet( enterprise ?? new EnterpriseModel() );
		}

		// POST: /Enterprise/GetSearchEnterprises
		[HttpPost]
		public JsonNetResult GetSearchEnterprises( int deviceId, int? categoryId, string text, string sort )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			EnterpriseSearchSortTypes sortType = ( EnterpriseSearchSortTypes )int.Parse( sort );

			List<EnterpriseModel> enterprises = new List<EnterpriseModel>();

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					enterprises = ConnectCmsRepository.EnterpriseRepository.GetSearchEnterprises( deviceId, location.Latitude, location.Longitude, hotel.RadiusInMeters, categoryId, text, sortType );
			}

			return JsonNet( enterprises ?? new List<EnterpriseModel>() );
		}

		// POST: /Enterprise/UpdateEnterprise
		[HttpPost]
		public JsonNetResult UpdateEnterprise( int deviceId, EnterpriseModel enterprise, EnterpriseLocationModel enterpriseLocation )
		{
			ConnectCmsRepository.EnterpriseRepository.UpdateEnterprise( deviceId, enterprise, enterpriseLocation );

			return JsonNet( true );
		}

		// POST: /Enterprise/UpdateEnterpriseRecommendation
		[HttpPost]
		public JsonNetResult UpdateEnterpriseRecommendation( int deviceId, int enterpriseId, int? enterpriseLocationId, List<int> addedCategoryIds, List<int> removedCategoryIds )
		{
			ConnectCmsRepository.EnterpriseRepository.UpdateEnterprise( deviceId, enterpriseId, enterpriseLocationId, addedCategoryIds, removedCategoryIds );

			return JsonNet( true );
		}
	}
}