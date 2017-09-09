using ConnectCMS.Models.Account;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Utils;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Image = MonsciergeDataModel.Image;

namespace ConnectCMS.Repositories
{
	public class HotelBrandRepository : ChildRepository
	{
		public HotelBrandRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public HotelBrandRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context, MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public HotelBrand GetHotelBrand( int id, string includes = "" )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();
			var brand = Rp.ExecuteAction( () =>
			{
				var h = ProxylessContext.HotelBrands.Where( x => x.PKID == id );
				return includes.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Aggregate( h, ( current, include ) => current.Include( include ) );
			} );

			return brand.FirstOrDefault();
		}

		public List<HotelBrand> SearchHotelBrands( string searchText, string includes = "" )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();
			var brands = Rp.ExecuteAction( () =>
			{
				var h = ProxylessContext.HotelBrands.Where( x => x.Name.ToLower().Contains( searchText.ToLower() ) );
				return includes.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Aggregate( h, ( current, include ) => current.Include( include ) );
			} );

			return brands.ToList();
		}

		public void GetLoginTheme( LoginModel model, string brand, string hotel )
		{
			HotelBrand hb = null;
			if( !string.IsNullOrEmpty( brand ) )
			{
				hb =
					ProxylessContext.HotelBrands.Include( x => x.HotelBrandImageMaps.Select( y => y.Image ) ).Include( x => x.LogoOnWhiteImage ).FirstOrDefault( x => x.HotelBrandSlugs.Any( y => y.HotelBrandSlugName.ToLower() == brand.ToLower() ) );

				if( hb != null )
				{
					if( hb.ThemeColor1.HasValue )
					{
						var theme1 = MediaUtilities.ColorFromInt( hb.ThemeColor1.Value );
						if( theme1 != null )
							model.ThemeColor1 = ColorTranslator.ToHtml( theme1.Value );
					}
					if( hb.ThemeColor2.HasValue )
					{
						var theme2 = MediaUtilities.ColorFromInt( hb.ThemeColor2.Value );
						if( theme2 != null )
							model.ThemeColor2 = ColorTranslator.ToHtml( theme2.Value );
					}
					if( hb.HotelBrandImageMaps.Any() )
						model.ThemeImages = hb.HotelBrandImageMaps.Select( x => x.Image ).ToList();
					if( hb.LogoOnWhiteImage != null )
						model.LogoOnWhiteImage = hb.LogoOnWhiteImage;
				}
			}
			Hotel h = null;
			if( !string.IsNullOrEmpty( hotel ) )
			{
				h =
					ProxylessContext.Hotels
						.Include( x => x.HotelImageMaps.Select( y => y.Image ) )
						.Include( x => x.HotelDetail )
						.Include( x => x.HotelDetail.LogoOnWhiteImage )
						.FirstOrDefault( x => x.HotelSlugs.Any( y => y.HotelSlugName.ToLower() == hotel.ToLower() ) );

				if( h != null )
				{
					if( h.HotelDetail.ThemeColor1.HasValue )
					{
						var theme1 = MediaUtilities.ColorFromInt( h.HotelDetail.ThemeColor1.Value );
						if( theme1 != null )
							model.ThemeColor1 = ColorTranslator.ToHtml( theme1.Value );
					}
					if( h.HotelDetail.ThemeColor2.HasValue )
					{
						var theme2 = MediaUtilities.ColorFromInt( h.HotelDetail.ThemeColor2.Value );
						if( theme2 != null )
							model.ThemeColor2 = ColorTranslator.ToHtml( theme2.Value );
					}
					if( h.HotelImageMaps.Any() )
						model.ThemeImages = h.HotelImageMaps.Select( x => x.Image ).ToList();
					if( h.HotelDetail.LogoOnWhiteImage != null )
						model.LogoOnWhiteImage = h.HotelDetail.LogoOnWhiteImage;
				}
			}

			if( model.ThemeColor1 == null )
				model.ThemeColor1 = "#009cd8";
			if( model.ThemeColor2 == null )
				model.ThemeColor2 = "#008abf";
			if( model.ThemeImages == null )
				model.ThemeImages = new List<Image>
				{
					new Image{PKID = 773954, Path = "B701632D780EA89A4A612D311CDF4DF0883BB7E8.jpg", Width = 1600, Height = 1000},
					new Image{PKID = 773955, Path = "191620E574A337AE1F7E11EA8378F5AFB03203CF.jpg", Width = 1600, Height = 1000},
					new Image{PKID = 773956, Path = "598CCF5D989F40D7B2C66EF7B1694C9E9EF15AE1.jpg", Width = 1600, Height = 1000},
					new Image{PKID = 773957, Path = "5D9B5B360F0B0A9D4A574EEE951C98D2928A71FE.jpg", Width = 1600, Height = 1000}
				};
			if( model.LogoOnWhiteImage == null )
				model.LogoOnWhiteImage = new Image { PKID = 773958, Path = "309655E0AF902B6CECCEF2CBE68C1777FA0754DD.png", Width = 540, Height = 90 };

			var customCreateAccountLinks = new Dictionary<string, string>
			{
				{ "dolce", "https://signup.monscierge.com/Plans/Full?brand=dolce" },
				{ "ibis", "https://signup.monscierge.com/Plans/Full?brand=ibis" },
				{ "novotel", "https://signup.monscierge.com/Plans/Full?brand=novotel" },
				{ "ihg", "https://signup.monscierge.com/Plans/Full?brand=ihg" },
				{ "warwick", "https://signup.monscierge.com/Custom/Warwick" },
				{ "wyndham", "https://signup.monscierge.com/Plans/Full?brand=wyndham" },
				{ "banyantree", "https://signup.monscierge.com/Plans/Full?brand=banyantree" },
				{ "nordic", "https://signup.monscierge.com/Plans/Full?brand=nordicchoice" },
				{ "bestwestern", "https://signup.monscierge.com/Plans/Full?brand=bestwestern" },
				{ "parkinn", "https://signup.monscierge.com/Plans/Full?brand=parkinn" },
				{ "carlsonrezidor", "https://signup.monscierge.com/Plans/Full?brand=carlsonrezidor" }
			};

			model.CreateAccountUrl = brand != null && customCreateAccountLinks.ContainsKey( brand ) ? customCreateAccountLinks[ brand ] : "https://signup.monscierge.com";
		}

		public HotelBrandImageMap MapHotelImage( string path, string name, int width, int height, int hotelBrandId )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var hotelBrand =
				ProxylessContext.HotelBrands
					.Include( x => x.HotelBrandImageMaps )
					.FirstOrDefault( h => h.PKID == hotelBrandId );
			if( hotelBrand == null )
				throw new DataException( "The section you are trying to modify does not exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			var image = new Image
			{
				FKAccount = user.FKAccount,
				Path = path,
				Width = width,
				Height = height,
				Name = name,
				IsActive = true
			};

			var newOrdinal = hotelBrand.HotelBrandImageMaps.Count > 0
				? hotelBrand.HotelBrandImageMaps.Max( x => x.Ordinal ) + 1
				: 1;

			var map = new HotelBrandImageMap
			{
				HotelBrand = hotelBrand,
				Image = image,
				FKHotelBrand = hotelBrand.PKID,
				Ordinal = newOrdinal
			};

			ProxylessContext.Images.Add( image );
			ProxylessContext.HotelBrandImageMaps.Add( map );
			hotelBrand.HotelBrandImageMaps.Add( map );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return map;
		}

		public void RemoveHotelImageMap( int mapId )
		{
			var map = ProxylessContext.HotelBrandImageMaps.FirstOrDefault( x => x.Pkid == mapId );
			if( map == null )
				throw new InvalidDataException( "The image map you are trying to remove doesn't exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			ProxylessContext.HotelBrandImageMaps.Remove( map );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public HotelBrandSlug AddHotelBrandSlug( int hotelBrandId, string name )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();

			if( ProxylessContext.HotelBrandSlugs.Any( x => x.HotelBrandSlugName.ToLower() == name.ToLower() ) )
				throw new DuplicateNameException( "A slug with this name already exists" );

			var hotelBrand = ProxylessContext.HotelBrands.Include( x => x.HotelBrandSlugs ).FirstOrDefault( x => x.PKID == hotelBrandId );
			if( hotelBrand == null )
				throw new InvalidDataException( "The Hotel Brand you are attempting to create a slug for does not exist" );

			var slug = new HotelBrandSlug { HotelBrandSlugName = name.ToLower(), HotelBrand = hotelBrand };
			ProxylessContext.HotelBrandSlugs.Add( slug );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return slug;
		}

		public void RemoveHotelBrandSlug( int slugId )
		{
			var slug = ProxylessContext.HotelBrandSlugs.FirstOrDefault( x => x.PKID == slugId );
			if( slug == null )
				throw new InvalidDataException( "The hotel brand slug you are trying to remove doesn't exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			ProxylessContext.HotelBrandSlugs.Remove( slug );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}
	}
}