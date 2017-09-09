using ConnectCMS.Models.Enterprise;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class BingController : ControllerBase
	{
		// POST: /Bing/GetBingAddressGeocode
		[HttpPost]
		public JsonNetResult GetBingAddressGeocode( string address1, string city, string state, string postalCode, string country )
		{
			// TODO: JD: Get culture
			var geocode = ConnectCmsRepository.BingRepository.GetBingAddressGeocode( address1, city, state, postalCode, country, "en-US" );

			return JsonNet( new { geocode.Latitude, geocode.Longitude } );
		}

		// POST: /Bing/GetBingAddressGeocode
		[HttpPost]
		public JsonNetResult GetBingLocationGeocode( Location location )
		{
			// TODO: JD: Get culture
			var newLocation = ConnectCmsRepository.BingRepository.UpdateBingAddressGeocode( location, "en-US" );

			return JsonNet( newLocation );
		}

		// POST: /Bing/GetSearchBingEnterprises
		[HttpPost]
		public JsonNetResult GetSearchBingEnterprises( int deviceId, List<string> excludeBingIds, string sort, string text )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			EnterpriseSearchSortTypes sortType = ( EnterpriseSearchSortTypes )int.Parse( sort );

			List<EnterpriseModel> enterprises = new List<EnterpriseModel>();

			if( hotel != null )
			{
				var location = hotel.Location;

				if( location != null )
					// TODO: JD: Get culture
					enterprises = ConnectCmsRepository.BingRepository.GetSearchBingEnterprises( location.Latitude, location.Longitude, hotel.RadiusInKilometers, text, sortType, "en-US", excludeBingIds );
			}

			return JsonNet( enterprises );
		}
	}
}