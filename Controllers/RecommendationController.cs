using ConnectCMS.Models.ContactUser;
using ConnectCMS.Models.Enterprise;
using Monscierge.Utilities;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class RecommendationController : ControllerBase
	{
		// GET: /Recommendation/
		public ActionResult Index()
		{
			return View();
		}

		// GET: /Recommendation/BlacklistRecommendPopup
		public ActionResult BlacklistRecommendPopup()
		{
			return PartialView( "_BlacklistRecommendPopup" );
		}

		// GET: /Recommendation/Enterprise
		public ActionResult Enterprise()
		{
			return PartialView( "_Enterprise" );
		}

		// GET: /Recommendation/RecommendReorder
		public ActionResult RecommendReorder()
		{
			return PartialView( "_RecommendReorder" );
		}

		// POST: /Recommendation/DeleteBlacklist
		[HttpPost]
		public JsonNetResult DeleteBlacklist( int deviceId, int? enterpriseId, int? enterpriseLocationId )
		{
			bool result = false;

			if( enterpriseId.HasValue )
			{
				result = true;

				ConnectCmsRepository.RecommendationRepository.DeleteEnterpriseBlacklist( deviceId, enterpriseId.Value );
			}
			else
			{
				result = true;

				ConnectCmsRepository.RecommendationRepository.DeleteEnterpriseLocationBlacklist( deviceId, enterpriseLocationId.Value );
			}

			return JsonNet( result );
		}

		// POST: /Recommendation/GetEnterprisesWithTipsForHotel
		[HttpPost]
		public JsonNetResult GetEnterprisesWithTipsForHotel( int deviceId )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			List<EnterpriseModel> enterprises = null;

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					enterprises = ConnectCmsRepository.RecommendationRepository.GetEnterprisesWithTipsForHotel( deviceId, location.Latitude, location.Longitude, hotel.RadiusInMeters );
			}

			return JsonNet( enterprises ?? new List<EnterpriseModel>() );
		}

		// POST: /Recommendation/GetRecommendationContactUserSettings
		[HttpPost]
		public JsonNetResult GetRecommendationContactUserSettings()
		{
			var recommendationContactUserSettings = GetContactUserSettings( new List<ContactUserSettingKeys>()
			{
				ContactUserSettingKeys.RecommendationSearchHelpAddVisible,
				ContactUserSettingKeys.RecommendationSearchHelpNoBingVisible,
				ContactUserSettingKeys.RecommendationSearchHelpSearchBingVisible,
				ContactUserSettingKeys.RecommendationSearchHelpSearchVisible,
				ContactUserSettingKeys.RecommendationRecommendedHelpReorderVisible
			} );

			return JsonNet( recommendationContactUserSettings ?? new List<ContactUserSettingModel>() );
		}

		// POST: /Recommendation/GetRecommendedEnterprisesOnDevice
		[HttpPost]
		public JsonNetResult GetRecommendedEnterprisesOnDevice( int deviceId )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			List<EnterpriseModel> enterprises = null;

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					enterprises = ConnectCmsRepository.RecommendationRepository.GetRecommendedEnterprisesOnDevice( deviceId, location.Latitude, location.Longitude, hotel.RadiusInMeters );
			}

			return JsonNet( enterprises ?? new List<EnterpriseModel>() );
		}

		// POST: /Recommendation/GetSearchBlacklistedEnterprisesOnDevice
		[HttpPost]
		public JsonNetResult GetSearchBlacklistedEnterprisesOnDevice( int deviceId, int? categoryId, string text )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			List<EnterpriseModel> enteprises = null;

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					enteprises = ConnectCmsRepository.RecommendationRepository.GetSearchBlacklistedEnterprisesOnDevice( deviceId, location.Latitude, location.Longitude, hotel.RadiusInMeters, categoryId, text );
			}

			return JsonNet( enteprises ?? new List<EnterpriseModel>() );
		}

		// POST: /Recommendation/GetSearchRecommendedEnterprisesOnDevice
		[HttpPost]
		public JsonNetResult GetSearchRecommendedEnterprisesOnDevice( int deviceId, int? categoryId, string text )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			List<EnterpriseModel> enterprises = null;

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					enterprises = ConnectCmsRepository.RecommendationRepository.GetSearchRecommendedEnterprisesOnDevice( deviceId, location.Latitude, location.Longitude, hotel.RadiusInMeters, categoryId, text );
			}

			return JsonNet( enterprises ?? new List<EnterpriseModel>() );
		}

		// POST: /Recommendation/InsertBlacklist
		[HttpPost]
		public JsonNetResult InsertBlacklist( int deviceId, int? enterpriseId, int? enterpriseLocationId )
		{
			bool result = false;

			if( enterpriseId.HasValue )
			{
				result = true;

				ConnectCmsRepository.RecommendationRepository.InsertEnterpriseBlacklist( deviceId, enterpriseId.Value );
			}
			else if( enterpriseLocationId.HasValue )
			{
				result = true;

				ConnectCmsRepository.RecommendationRepository.InsertEnterpriseLocationBlacklist( deviceId, enterpriseLocationId.Value );
			}

			return JsonNet( result );
		}

		// POST: /Recommendation/UpdateRecommendationReorder
		[HttpPost]
		public JsonNetResult UpdateRecommendationReorder( int deviceId, List<EnterpriseModel> enterprises )
		{
			ConnectCmsRepository.RecommendationRepository.UpdateRecommendationReorder( deviceId, enterprises );

			return JsonNet( true );
		}
	}
}