using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public, ApplyToStateMachine = false )]
	public class HotelBrandController : ControllerBase
	{
		// GET: Brand
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public JsonNetResult SearchHotelBrands( string searchText, string includes )
		{
			var hotels = new List<HotelBrand>();
			int id;
			if( int.TryParse( searchText, out id ) )
			{
				var hotel = ConnectCmsRepository.HotelBrandRepository.GetHotelBrand( id, includes );
				if( hotel != null )
					hotels.Add( hotel );
			}
			else
			{
				hotels = ConnectCmsRepository.HotelBrandRepository.SearchHotelBrands( searchText, includes );
			}

			return JsonNet( hotels );
		}

		[HttpPost]
		public JsonNetResult MapHotelBrandImage( string path, string name, int width, int height, int hotelBrandId )
		{
			var imageMap = ConnectCmsRepository.HotelBrandRepository.MapHotelImage( path, name, width, height, hotelBrandId );
			return JsonNet( imageMap );
		}

		[HttpPost]
		public JsonNetResult RemoveHotelBrandImageMap( int mapId )
		{
			ConnectCmsRepository.HotelBrandRepository.RemoveHotelImageMap( mapId );
			return JsonNet( true );
		}

		[HttpPost]
		public JsonNetResult AddHotelBrandSlug( int hotelBrandId, string name )
		{
			var slug = ConnectCmsRepository.HotelBrandRepository.AddHotelBrandSlug( hotelBrandId, name );
			return JsonNet( slug );
		}

		[HttpPost]
		public JsonNetResult RemoveHotelBrandSlug( int slugId )
		{
			ConnectCmsRepository.HotelBrandRepository.RemoveHotelBrandSlug( slugId );
			return JsonNet( true );
		}
	}
}