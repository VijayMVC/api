using ConnectCMS.Extensions;
using ConnectCMS.Models;
using ConnectCMS.Models.Hotel;
using ConnectCMS.Models.Image;
using ConnectCMS.Models.Location;
using ConnectCMS.Models.Setup;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Utils;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using MonsciergeSFWrapper.SF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Serialization.Configuration;

namespace ConnectCMS.Repositories
{
	public class HotelRepository : ChildRepository
	{
		public HotelRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public HotelRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public HotelModel GetHotelFromDevice( int deviceId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			var hotel = Rp.ExecuteAction( () => ( from h in Context.Hotels
												  let hd = h.HotelDetail
												  where h.Devices.Select( d => d.PKID ).Contains( deviceId )
												  select new HotelModel
												  {
													  Location = new LocationModel
													  {
														  Address = hd.Address,
														  City = hd.City,
														  ISOCountryCode = hd.ISOCountryCode,
														  Latitude = h.HotelDetail.Latitude,
														  Longitude = h.HotelDetail.Longitude,
														  PostalCode = hd.Zip,
														  ISOStateCode = hd.State,
													  },
													  Name = h.Name,
													  PKID = h.PKID,
													  RadiusInMiles = h.HotelDetail.Radius
												  } ).FirstOrDefault() );

			return hotel;
		}

		public void FillPropertyInfo( PropertyInfoModel propertyInfo )
		{
			var account = RootRepository.AccountRepository.GetCurrentLoggedInAccount();
			var hotel = account.Hotels.FirstOrDefault();
			if( hotel == null || hotel.HotelDetail == null )
				return;

			propertyInfo.FKHotel = hotel.PKID;
			propertyInfo.Name = hotel.Name;
			propertyInfo.Phone = hotel.HotelDetail.Phone;
			propertyInfo.Location = new LocationModel
			{
				Address = hotel.HotelDetail.Address,
				City = hotel.HotelDetail.City,
				ISOStateCode = hotel.HotelDetail.State,
				PostalCode = hotel.HotelDetail.Zip,
				ISOCountryCode = hotel.HotelDetail.ISOCountryCode,
			};
		}

		public void FillAboutHotel( AboutHotelModel aboutHotel )
		{
			var account = RootRepository.AccountRepository.GetCurrentLoggedInAccount();
			var hotel = account.Hotels.FirstOrDefault();
			if( hotel == null || hotel.HotelDetail == null )
				return;

			aboutHotel.Description = hotel.HotelDetail.Description;
			aboutHotel.Logo = hotel.HotelDetail.LogoImage != null ? new ImageModel( hotel.HotelDetail.LogoImage ) : null;
		}

		public void FillMobileBackground( MobileBackgroundModel mobileBackground )
		{
			var account = RootRepository.AccountRepository.GetCurrentLoggedInAccount();
			var hotel = account.Hotels.FirstOrDefault();
			if( hotel == null || hotel.HotelDetail == null )
				return;

			mobileBackground.MobileBackground = hotel.HotelDetail.MobileBackgroundImage != null ? new ImageModel( hotel.HotelDetail.MobileBackgroundImage ) : null;
		}

		public void UpdatePropertyInfo( PropertyInfoModel model )
		{
			var account = RootRepository.AccountRepository.GetCurrentLoggedInAccount();
			var hotel = account.Hotels.FirstOrDefault();
			//TODO: JD: Throw Error
			if( hotel == null || hotel.HotelDetail == null )
				return;

			var nameHasChanges = !string.Equals( hotel.Name, model.Name );
			var phoneHasChanges = !string.Equals( hotel.HotelDetail.Phone, model.Phone );

			hotel.Name = model.Name;
			hotel.HotelDetail.Phone = model.Phone;

			var locationHasChanges = false;
			if( model.Location != null )
			{
				locationHasChanges = hotel.HotelDetail.LocationIsDifferent( model.Location );

				if( locationHasChanges )
				{
					hotel.HotelDetail.Address = model.Location.Address;
					hotel.HotelDetail.City = model.Location.City;
					hotel.HotelDetail.State = model.Location.State != null ? model.Location.State.ISOStateCode : null;
					hotel.HotelDetail.Zip = model.Location.PostalCode;
					hotel.HotelDetail.ISOCountryCode = model.Location.Country != null
						? model.Location.Country.ISOCountryCode
						: null;

					// Update the hotel's lat/long
					try
					{
						var geoCodeResult = MonsciergeServiceUtilities.VendorServices.Geocode.DoGeocode( model.Location.FullAddress, "", true );
						if( geoCodeResult != null && geoCodeResult.Any() )
						{
							var result = geoCodeResult[ 0 ];
							if( result != null )
							{
								hotel.HotelDetail.Latitude = ( float )( result.Latitude.HasValue ? result.Latitude.Value : 0 );
								hotel.HotelDetail.Longitude = ( float )( result.Longitude.HasValue ? result.Longitude.Value : 0 );
							}
						}
					}
					catch( Exception ex )
					{
						Logger.LogError( ex.ToString() );
					}
				}
			}

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			// If there were any changes to the property info, SalesForce needs to be updated as well.
			var hasChanges = ( nameHasChanges || phoneHasChanges || locationHasChanges );

			if( !hasChanges )
				return;

			var contactUser = RootRepository.SecurityRepository.GetLoggedInUser();
			var sfWrapper = new MonsciergeSFWrapper.SFWrapper();
			try
			{
				sfWrapper.Connect();
				var existingLead = sfWrapper.GetLeadByEmail( contactUser.Email );
				if( existingLead != null && string.Equals( existingLead.LeadSource, "signup.monscierge.com" ) )
				{
					var lead = new Lead
					{
						Id = existingLead.Id,
					};

					var isSelectPlan = string.Equals( existingLead.connect_product__c, "Select", StringComparison.OrdinalIgnoreCase );

					if( nameHasChanges )
					{
						lead.property_name__c = model.Name;

						if( isSelectPlan )
						{
							lead.billing_company_name__c = model.Name;
							lead.Company = model.Name;
						}
					}

					if( phoneHasChanges )
					{
						lead.Phone = model.Phone;
						if( isSelectPlan )
						{
							lead.billing_phone__c = model.Phone;
						}
					}

					if( model.Location != null && locationHasChanges )
					{
						lead.address_1__c = model.Location.Address1;
						lead.address_2__c = model.Location.Address2;
						lead.hotel_city__c = model.Location.City;
						lead.hotel_state__c = model.Location.State == null ? "" : model.Location.State.ISOStateCode;
						lead.hotel_zip__c = model.Location.PostalCode;
						lead.hotel_country__c = model.Location.Country == null ? "" : model.Location.Country.ISOCountryCode;

						if( isSelectPlan )
						{
							lead.billing_address__c = model.Location.Address1;
							lead.billing_address_2__c = model.Location.Address2;
							lead.billing_city__c = model.Location.City;
							lead.billing_state__c = model.Location.State == null ? "" : model.Location.State.ISOStateCode;
							lead.billing_postal_code__c = model.Location.PostalCode;
							lead.billing_country__c = model.Location.Country == null ? "" : model.Location.Country.ISOCountryCode;
						}
					}

					var result = sfWrapper.UpdateLead( lead );
					if( result.errors == null )
						return;

					result.errors.ForEach( e => Logger.LogInfo( e.message ) );
					throw new Exception(
						"Unable to update SalesForce lead record. Review preceding info messages for additional details." );
				}
			}
			catch( Exception ex )
			{
				Logger.LogError( ex.ToString() );
			}
			finally
			{
				sfWrapper.Disconnect();
			}
		}

		public void UpdateAboutHotel( AboutHotelModel model )
		{
			var account = RootRepository.AccountRepository.GetCurrentLoggedInAccount();
			var hotel = account.Hotels.FirstOrDefault();

			//TODO: JD: Throw Error
			if( hotel == null || hotel.HotelDetail == null )
				return;

			var logo = model.Logo != null ? Rp.ExecuteAction( () => ( from cI in Context.Images
																	  where cI.PKID == model.Logo.PKID
																	  select cI ) ).FirstOrDefault() : null;

			if( logo != null )
				hotel.HotelDetail.LogoImage = logo;

			hotel.HotelDetail.Description = model.Description;

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void UpdateMobileBackground( MobileBackgroundModel model )
		{
			var account = RootRepository.AccountRepository.GetCurrentLoggedInAccount();
			var hotel = account.Hotels.FirstOrDefault();

			//TODO: JD: Throw Error
			if( hotel == null || hotel.HotelDetail == null )
				return;

			var mobileBackground = model.MobileBackground != null ? Rp.ExecuteAction(
					() => ( from cI in Context.Images
							where cI.PKID == model.MobileBackground.PKID
							select cI ) ).FirstOrDefault() : null;

			if( mobileBackground != null )
				hotel.HotelDetail.MobileBackgroundImage = mobileBackground;

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public MobileApp GetMobileAppFromAppShort( string appShort )
		{
			if( appShort == null )
				throw new ArgumentNullException( "appShort" );

			var mobileApp = ProxylessContext.MobileApps
				.FirstOrDefault( x => x.AppShort == appShort );

			if( mobileApp == null )
				throw new Exception( "This hotel doesn't have a mobile app" );

			return mobileApp;
		}

		public Hotel GetHotelSetup( int? deviceId, int? hotelId )
		{
			if( hotelId.HasValue )
			{
				RootRepository.DeviceRepository.GetAutherizedDeviceByHotel( hotelId.Value );
			}
			else
			{
				if( deviceId.HasValue )
				{
					RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId.Value );
					var device = RootRepository.DeviceRepository.GetDevice( deviceId.Value );
					hotelId = device.FKHotel;
				}
				else
				{
					var fdevice = RootRepository.DeviceRepository.GetAdminDeviceForLoggedInUser();
					var fHotel = fdevice == null ? null : fdevice.Hotel;
					if( fHotel == null )
						throw new Exception( "This user does not have access to a device" );
					RootRepository.SecurityRepository.AssertDeviceAuthorization( fHotel.Devices.First().PKID );
					hotelId = fHotel.PKID;
				}
			}
			var hotel =
				Rp.ExecuteAction(
					() => ( from h in ProxylessContext.Hotels
								.Include( h => h.HotelDetail )
								.Include( h => h.HotelDetail.LogoImage )
								.Include( h => h.HotelDetail.LogoOnWhiteImage )
								.Include( h => h.HotelDetail.MobileBackgroundImage )
							where h.PKID == hotelId.Value
							select h ).FirstOrDefault() );

			hotel.HotelDetail.Location.Country =
				UtilityRepository.GetAllCountries().FirstOrDefault( c => c.ISOCountryCode == hotel.HotelDetail.ISOCountryCode );
			hotel.HotelDetail.Location.State = hotel.HotelDetail.Location.Country != null
				? hotel.HotelDetail.Location.Country.States.FirstOrDefault( s => s.ISOStateCode == hotel.HotelDetail.State )
				: UtilityRepository.GetAllStates().FirstOrDefault( s => s.ISOStateCode == hotel.HotelDetail.State );

			return hotel;
		}

		public Hotel UpdateHotelSetup( Hotel model )
		{
			var hotel =
				Rp.ExecuteAction(
					() => ( from h in ProxylessContext.Hotels
								.Include( h => h.HotelDetail )
								.Include( h => h.HotelDetail.LogoImage )
								.Include( h => h.HotelDetail.MobileBackgroundImage )
							where h.PKID == model.PKID
							select h ).FirstOrDefault() );

			hotel.Name = model.Name;

			string newLocalizedDescription, newDescription, newDescriptionLanguage;
			Localization.SetLocalizedText( hotel.HotelDetail.LocalizedDescription, hotel.HotelDetail.Description,
				hotel.HotelDetail.DescriptionLanguage, null, model.HotelDetail.Description, out newLocalizedDescription,
				out newDescription, out newDescriptionLanguage );

			hotel.HotelDetail.Description = newDescription;
			hotel.HotelDetail.LocalizedDescription = newLocalizedDescription;
			hotel.HotelDetail.DescriptionLanguage = newDescriptionLanguage;

			hotel.HotelDetail.FKLogoImage = model.HotelDetail.FKLogoImage;
			hotel.HotelDetail.FKMobileBackgroundImage = model.HotelDetail.FKMobileBackgroundImage;

			hotel.HotelDetail.Phone = model.HotelDetail.Phone;

			hotel.HotelDetail.Address = model.HotelDetail.Location.Address;
			hotel.HotelDetail.City = model.HotelDetail.Location.City;
			hotel.HotelDetail.ISOCountryCode = model.HotelDetail.Location.ISOCountryCode;
			hotel.HotelDetail.State = model.HotelDetail.Location.ISOStateCode;
			hotel.HotelDetail.Zip = model.HotelDetail.Location.PostalCode;

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return GetHotelSetup( null, model.PKID );
		}

		public void MapLogoImage( int deviceId, Image image )
		{
			RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );
			var hotelDetail =
				Rp.ExecuteAction( () => ( from d in ProxylessContext.Devices where d.PKID == deviceId select d.Hotel.HotelDetail ) )
					.First();

			hotelDetail.LogoOnWhiteImage = image;
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void MapMobileBackgroundImage( int deviceId, Image image )
		{
			RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );
			var hotelDetail =
				Rp.ExecuteAction( () => ( from d in ProxylessContext.Devices where d.PKID == deviceId select d.Hotel.HotelDetail ) )
					.First();

			hotelDetail.MobileBackgroundImage = image;
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public bool HasCompletedSetup()
		{
			var device = RootRepository.DeviceRepository.GetAdminDeviceForLoggedInUser();
			return device == null || device.Hotel.HotelDetail.SetupComplete;
		}

		public void CompleteSetup()
		{
			var device = RootRepository.DeviceRepository.GetAdminDeviceForLoggedInUser();
			if( device != null )
			{
				var hd = ProxylessContext.HotelDetails.First( x => x.PKID == device.FKHotel );
				hd.SetupComplete = true;
				ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			}
		}

		public Hotel GetHotel( int id, string includes = "" )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();
			var hotels = Rp.ExecuteAction( () =>
			{
				var h = ProxylessContext.Hotels.Where( x => x.PKID == id );
				return includes.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Aggregate( h, ( current, include ) => current.Include( include ) );
			} );

			return hotels.FirstOrDefault();
		}

		public IQueryable<Hotel> GetHotelQuery( int id )
		{
			return Rp.ExecuteAction( () => ProxylessContext.Hotels.Where( x => x.PKID == id ) );
		}

		public IQueryable<Hotel> SearchHotelQuery( string searchText )
		{
			return Rp.ExecuteAction( () => ProxylessContext.Hotels.Where( x => x.Name.ToLower().Contains( searchText.ToLower() ) ) );
		}

		public List<Hotel> SearchHotels( string searchText, string includes = "" )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();
			var hotels = Rp.ExecuteAction( () =>
			{
				var h = ProxylessContext.Hotels.Where( x => x.Name.ToLower().Contains( searchText.ToLower() ) );
				return includes.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Aggregate( h, ( current, include ) => current.Include( include ) );
			} );

			return hotels.ToList();
		}

		public HotelImageMap MapHotelImage( string path, string name, int width, int height, int hotelId )
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var hotel =
				ProxylessContext.Hotels
					.Include( x => x.HotelImageMaps )
					.Include( x => x.Devices )
					.FirstOrDefault( h => h.PKID == hotelId );
			if( hotel == null )
				throw new DataException( "The section you are trying to modify does not exist" );

			if( !hotel.Devices.Any( x => RootRepository.SecurityRepository.HasDevicePermissions( x.PKID ) ) )
				throw new SecurityException( "You are not authorized to access this hotel" );

			var image = new Image
			{
				FKAccount = user.FKAccount,
				Path = path,
				Width = width,
				Height = height,
				Name = name,
				IsActive = true
			};

			var newOrdinal = hotel.HotelImageMaps.Count > 0
				? hotel.HotelImageMaps.Max( x => x.Ordinal ) + 1
				: 1;

			var map = new HotelImageMap
			{
				Hotel = hotel,
				Image = image,
				FKHotel = hotel.PKID,
				Ordinal = newOrdinal
			};

			ProxylessContext.Images.Add( image );
			ProxylessContext.HotelImageMaps.Add( map );
			hotel.HotelImageMaps.Add( map );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return map;
		}

		public void RemoveHotelImageMap( int mapId )
		{
			var map = ProxylessContext.HotelImageMaps.Include( x => x.Hotel.Devices ).FirstOrDefault( x => x.Pkid == mapId );
			if( map == null )
				throw new InvalidDataException( "The image map you are trying to remove doesn't exist" );

			RootRepository.SecurityRepository.AssertDeviceAuthorization( map.Hotel.Devices.First().PKID );

			ProxylessContext.HotelImageMaps.Remove( map );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public HotelSlug AddHotelSlug( int hotelId, string name )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();

			if( ProxylessContext.HotelSlugs.Any( x => x.HotelSlugName.ToLower() == name.ToLower() ) )
				throw new DuplicateNameException( "A slug with this name already exists" );

			var hotel = ProxylessContext.Hotels.Include( x => x.HotelSlugs ).FirstOrDefault( x => x.PKID == hotelId );
			if( hotel == null )
				throw new InvalidDataException( "The Hotel you are attempting to create a slug for does not exist" );

			var slug = new HotelSlug { HotelSlugName = name.ToLower(), Hotel = hotel };
			ProxylessContext.HotelSlugs.Add( slug );
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return slug;
		}

		public void RemoveHotelSlug( int slugId )
		{
			var slug = ProxylessContext.HotelSlugs.FirstOrDefault( x => x.PKID == slugId );
			if( slug == null )
				throw new InvalidDataException( "The hotel slug you are trying to remove doesn't exist" );

			RootRepository.SecurityRepository.AssertSuperAdmin();

			ProxylessContext.HotelSlugs.Remove( slug );

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public void UpdateHideConnectWebHeaderTitle( int hotelId, bool value )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();

			var hotelDetail = ProxylessContext.HotelDetails.FirstOrDefault( x => x.PKID == hotelId );
			if( hotelDetail == null )
				throw new InvalidDataException( "The Hotel you are attempting to update does not exist" );

			hotelDetail.HideConnectWebHeaderTitle = value;
			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		public Image UpdateConnectWebHeaderLogoImage( int hotelId, int? imageId )
		{
			RootRepository.SecurityRepository.AssertSuperAdmin();

			var hotelDetail = ProxylessContext.HotelDetails.FirstOrDefault( x => x.PKID == hotelId );
			if( hotelDetail == null )
				throw new InvalidDataException( "The Hotel you are attempting to update does not exist" );

			Image image = null;
			if( imageId.HasValue )
			{
				image = ProxylessContext.Images.FirstOrDefault( x => x.PKID == imageId.Value );
				if( image == null )
					throw new InvalidDataException( "The Logo Image you are attempting to add does not exist" );

				hotelDetail.FKLogoConnectWebHeaderImage = image.PKID;
			}
			else
			{
				hotelDetail.FKLogoConnectWebHeaderImage = null;
			}

			ProxylessContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			return image;
		}

		public PermissionResult CheckHotelPermission( int userId, int hotelId )
		{
			var hotel = GetHotelQuery( hotelId ).Include( x => x.Devices ).FirstOrDefault();
			if( hotel == null )
			{
				return new PermissionResult( PermissionResults.InvalidObject,
					"The Hotel you are trying to access does not exist",
					new[] { new KeyValuePair<string, object>( "hotelId", hotelId ) }
					);
			}
			return CheckHotelPermission( userId, hotel );
		}

		public PermissionResult CheckHotelPermission( int userId, Hotel hotel )
		{
			if( hotel.Devices.Where( d => d != null ).Any( device => RootRepository.DeviceRepository.CheckDevicePermission( userId, device.PKID ).Result == PermissionResults.Authorized ) )
			{
				return new PermissionResult { Result = PermissionResults.Authorized };
			}

			return new PermissionResult( PermissionResults.Unauthorized, "You are unauthorized to access this hotel.",
				new[] { new KeyValuePair<string, object>( "userId", userId ), new KeyValuePair<string, object>( "hotelId", hotel.PKID ), } );
		}

		public bool HasHotelPermission( int userId, int hotelId )
		{
			var hotel = GetHotelQuery( hotelId ).Include( x => x.Devices ).FirstOrDefault();
			return hotel.Devices.Where( d => d != null ).Any( device => RootRepository.DeviceRepository.HasDevicePermission( userId, device.PKID ) );
		}

		public bool HasHotelPermission( int userId, Hotel hotel )
		{
			return hotel.Devices.Where( d => d != null ).Any( device => RootRepository.DeviceRepository.HasDevicePermission( userId, device ) );
		}
	}
}