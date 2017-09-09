using ConnectCMS.Repositories;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using Newtonsoft.Json;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public, ApplyToStateMachine = false )]
	public class HotelController : ControllerBase
	{
		// GET: Hotel
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public JsonNetResult SearchHotels( string searchText, string includes )
		{
			var hotels = new List<Hotel>();
			int id;
			if( int.TryParse( searchText, out id ) )
			{
				var hotel = ConnectCmsRepository.HotelRepository.GetHotel( id, includes );
				if( hotel != null )
					hotels.Add( hotel );
			}
			else
			{
				hotels = ConnectCmsRepository.HotelRepository.SearchHotels( searchText, includes );
			}

			return JsonNet( hotels );
		}

		[HttpPost]
		public JsonNetResult MapHotelImage( string path, string name, int width, int height, int hotelId )
		{
			var imageMap = ConnectCmsRepository.HotelRepository.MapHotelImage( path, name, width, height, hotelId );
			return JsonNet( imageMap );
		}

		[HttpPost]
		public JsonNetResult RemoveHotelImageMap( int mapId )
		{
			ConnectCmsRepository.HotelRepository.RemoveHotelImageMap( mapId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult AddHotelSlug( int hotelId, string name )
		{
			var slug = ConnectCmsRepository.HotelRepository.AddHotelSlug( hotelId, name );
			return JsonNet( slug );
		}

		[HttpPost]
		public JsonNetResult RemoveHotelSlug( int slugId )
		{
			ConnectCmsRepository.HotelRepository.RemoveHotelSlug( slugId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult UpdateHideConnectWebHeaderTitle( int hotelId, bool value )
		{
			ConnectCmsRepository.HotelRepository.UpdateHideConnectWebHeaderTitle( hotelId, value );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult UpdateConnectWebHeaderImage( int hotelId, int? imageId )
		{
			var image = ConnectCmsRepository.HotelRepository.UpdateConnectWebHeaderLogoImage( hotelId, imageId );
			return JsonNet( new { Result = "OK", Image = image } );
		}
	}
}