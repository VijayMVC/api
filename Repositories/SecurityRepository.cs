#region using

using ConnectCMS.Models;
using ConnectCMS.Models.ContactUser;
using ConnectCMS.Repositories.Caching;
using ConnectCMS.Resources;
using ConnectCMS.Utils;
using LinqKit;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

#endregion using

namespace ConnectCMS.Repositories
{
	public class SecurityRepository : ChildRepository
	{
		#region Constructors

		public SecurityRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public SecurityRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		#endregion Constructors

		#region Methods

		#region User

		public ContactUser GetLoggedInUser()
		{
			var user = AuthenticationManager.User;
			var idClaim = user.Claims.FirstOrDefault( x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" );
			var cuClaim = user.Claims.FirstOrDefault( x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/contactuser" );

			if( idClaim == null )
				return null;

			var id = int.Parse( cuClaim != null ? cuClaim.Value : idClaim.Value );

			return Rp.ExecuteAction( () => ( from cu in Context.ContactUsers
												  .Include( "Account" )
												  .Include( "ContactUserSettings" )
												  .Include( "DefaultReachRole" )
												  .Include( "RequestUser" )
											 where cu.PKID == id
											 select cu ).FirstOrDefault() );
		}

		public void InsertOrUpdateContactUserSetting( int contactUserId, ContactUserSettingKeys key, object value )
		{
			var keyInt = ( int )key;
			string valueString = value != null ? value.ToString() : null;

			var contactUserSetting = Context.ContactUserSettings.FirstOrDefault( cus => cus.FKContactUser == contactUserId && cus.Key == keyInt );

			if( contactUserSetting == null )
			{
				Context.ContactUserSettings.Add( new ContactUserSetting()
				{
					FKContactUser = contactUserId,
					Key = keyInt,
					Value = valueString
				} );
			}
			else
			{
				contactUserSetting.Value = valueString;
			}

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
		}

		#region ResetPassword

		public void SendResetEmail( string userName )
		{
			Task.Run( () =>
			{
				var localContext = new MonsciergeEntities
				{
					CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds
				};

				var user = Rp.ExecuteAction( () =>
					( from cu in localContext.ContactUsers where cu.Email == userName select cu ).FirstOrDefault() );

				if( user == null )
					return;
				user.ResetPassword = Guid.NewGuid().ToString( "N" );
				user.ResetPasswordExpiration = DateTime.UtcNow.AddMinutes( 30 );

				Rp.ExecuteAction( () => localContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );

				RootRepository.EmailRepository.SendPasswordReset( user );
			} );
		}

		public bool IsValidResetToken( Guid guid )
		{
			var guidStr = guid.ToString( "N" );
			var user = Rp.ExecuteAction( () => ( from cu in Context.ContactUsers
												 where cu.ResetPassword == guidStr && cu.ResetPasswordExpiration > DateTime.UtcNow
												 select cu ).FirstOrDefault() );

			return user != null;
		}

		public bool IsValidResetToken( string token, out Guid guid )
		{
			Guid g;

			if( !Guid.TryParseExact( token, "N", out g ) )
			{
				guid = new Guid();
				return false;
			}
			var guidString = g.ToString( "N" );
			var user = Rp.ExecuteAction( () => ( from cu in Context.ContactUsers
												 where cu.ResetPassword == guidString && cu.ResetPasswordExpiration > DateTime.UtcNow
												 select cu ).FirstOrDefault() );
			guid = g;
			return user != null;
		}

		public bool ResetPassword( Guid resetGuid, string password1, string password2 )
		{
			if( password1 != password2 || IsWeakPassword( password1 ) )
			{
				return false;
			}
			var guidStr = resetGuid.ToString( "N" );
			var user = Rp.ExecuteAction( () => ( from cu in Context.ContactUsers
												 where cu.ResetPassword == guidStr && cu.ResetPasswordExpiration > DateTime.UtcNow
												 select cu ).FirstOrDefault() );

			if( user == null )
				return false;

			GenerateSaltAndHashForUser( user, password1 );

			user.ResetPassword = null;

			var profile = Context.Users.FirstOrDefault( x => x.FKContactUser == user.PKID );
			if( profile != null )
			{
				profile.PasswordHash = user.PasswordHash;
				profile.Salt = user.Salt;
				profile.LastModifiedOn = DateTime.UtcNow;
				profile.ResetExpiration = null;
				profile.ResetToken = null;
			}

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );

			return true;
		}

		#endregion ResetPassword

		#endregion User

		#region Device

		public bool HasDevicePermissions( int deviceId )
		{
			if( IsSuperAdmin() )
				return true;
			var user = GetLoggedInUser();
			return Rp.ExecuteAction( () => Context.GetDevicesForUser( user.PKID ).Select( ud => ud.PKID ).Contains( deviceId ) || IsSuperAdmin() );
		}

		internal void AssertDeviceAuthorization( int deviceId )
		{
			if( IsSuperAdmin() )
				return;
			Rp.ExecuteAction( () =>
			{
				if( Context.GetDevicesForUser( GetLoggedInUser().PKID ).Any( d => d.PKID == deviceId ) == false )
				{
					throw new SecurityException( "You are not authorized to access this device" );
				}
			} );
		}

		internal void AssertDeviceLinkedObjectAuthorization( Expression<Func<Device, bool>> condition, int? userId = null )
		{
			if( IsSuperAdmin( userId ) )
				return;
			Rp.ExecuteAction( () =>
			{
				if( Context.GetDevicesForUser( userId.HasValue ? userId : GetLoggedInUser().PKID ).AsExpandable().Any( d => condition.Invoke( d ) ) == false )
				{
					throw new SecurityException( "You are not authorized to perform this operation." );
				}
			} );
		}

		public bool IsSuperAdmin( int? userId = null )
		{
			var uId = userId.HasValue ? userId : GetLoggedInUser().PKID;
			return Rp.ExecuteAction( () => Context.ContactUsers.Any( cu => cu.PKID == uId && cu.FKDefaultReachRole == 6 ) );
		}

		public void AssertDevicePermissions( int? deviceId )
		{
			if( IsSuperAdmin() )
				return;
			if( !deviceId.HasValue || !HasDevicePermissions( deviceId.Value ) )
			{
				throw new SecurityException( "You are not authorized to access one of the devices" );
			}
		}

		public List<MenuItemModel> GetNavigationMenuItems( int? deviceId )
		{
			var menuItems = new List<MenuItemModel>();

			var user = GetLoggedInUser();

			if( deviceId.HasValue )
			{
				var device = RootRepository.DeviceRepository.GetDevice( deviceId.Value );

				menuItems.Add( new MenuItemModel
				{
					Text = ConnectCMSResources.Dashboard,
					Icon = "dashboard.png",
					IsSilverlight = false,
					UseCustomNavigation = true,
					Url = "Dashboard/Index",
					IsDefault = true
				} );
				if( user.DefaultReachRole.ManageSubDevices && !device.Hotel.HasMobileLite && !device.Hotel.HotelDetail.HasSVNiPad )
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.SubDevices,
						Icon = "sub_devices.png",
						IsSilverlight = true,
						UseCustomNavigation = true
					} );

				menuItems.Add( new MenuItemModel
				{
					Text = device.Hotel.HotelDetail.HasSVNiPad ? ConnectCMSResources.Home : ConnectCMSResources.HomePage,
					Icon = "home.png",
					IsSilverlight = true,
					UseCustomNavigation = true
				} );

				#region HotelInfo

				var hotelinfo = new MenuItemModel
				{
					Text =
						device.Hotel.HotelDetail.HasSVNiPad ? ConnectCMSResources.OnSiteInformation : ConnectCMSResources.HotelInformation,
					Icon = "information.png",
					IsSilverlight = true,
					UseCustomNavigation = true
				};

				hotelinfo.SubMenuItems.Add( new MenuItemModel
				{
					Text = device.Hotel.HotelDetail.HasSVNiPad ? ConnectCMSResources.AmenitiesAndDining : ConnectCMSResources.Amenities,
					Icon = null,
					IsSilverlight = true,
					UseCustomNavigation = true
				} );

				if( device.Hotel.HotelDetail.EnableEvents )
				{
					var eventsMenuItem = new MenuItemModel
					{
						Text = ConnectCMSResources.EventsCalendar,
						Icon = "calendar.png",
						IsSilverlight = false,
						Url = "/ConnectCMS/Events/",
						UseCustomNavigation = true,
						WithDeviceId = true
					};
					hotelinfo.SubMenuItems.Add( eventsMenuItem );

					if( IsSuperAdmin() )
					{
						eventsMenuItem.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.EventsImporter,
							IsSilverlight = false,
							Url = "/ConnectCMS/Events/EventsImporter",
							UseCustomNavigation = true,
							WithDeviceId = true
						} );
					}
				}

				hotelinfo.SubMenuItems.Add( new MenuItemModel
				{
					Text = ConnectCMSResources.HotelMap,
					Icon = "map.png",
					IsSilverlight = true,
					UseCustomNavigation = true
				} );

				if( device.Hotel.HotelDetail.EnableValet )
				{
					hotelinfo.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.Valet,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}

				if( device.Hotel.HotelDetail.EnableDirectory )
				{
					hotelinfo.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.Directories,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}

				if( device.Hotel.HotelDetail.EnableMenu && !device.Hotel.HotelDetail.HasSVNiPad )
				{
					hotelinfo.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.RoomServiceMenus,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );

					hotelinfo.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.MenuCharacteristics,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}

				if( device.Hotel.HotelDetail.EnableBreakfast2Go )
				{
					hotelinfo.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.Breakfast2Go,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}

				if( !device.Hotel.HasMobileLite )
				{
					menuItems.Add( hotelinfo );
				}

				#endregion HotelInfo

				#region MapsAndDirections

				var mapsAndDirections = new MenuItemModel
				{
					Text = device.Hotel.HotelDetail.HasSVNiPad ? ConnectCMSResources.Maps : ConnectCMSResources.MapsAndDirections,
					Icon = "map.png",
					IsSilverlight = true,
					UseCustomNavigation = true
				};
				mapsAndDirections.SubMenuItems.Add( new MenuItemModel
				{
					Text = ConnectCMSResources.DeviceLocation,
					Icon = null,
					IsSilverlight = true,
					UseCustomNavigation = true
				} );
				if( !device.Hotel.HasMobileLite )
				{
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.BikeMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.BusMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.HikingMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.LocalMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.RunningMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.SubwayMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.TrainMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					mapsAndDirections.SubMenuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.WalkingMaps,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}
				menuItems.Add( mapsAndDirections );

				#endregion MapsAndDirections

				if( device.Hotel.HotelDetail.HasLocalRecommendation )
				{
					menuItems.Add( new MenuItemModel
					{
						Text =
							device.Hotel.HotelDetail.HasSVNiPad ? ConnectCMSResources.ThingsToDo : ConnectCMSResources.LocalRecommendations,
						Icon = "recommended.png",
						//IsSilverlight = false,
						IsSilverlight = true,
						UseCustomNavigation = true,
						//Url = "Recommendation/Index"
					} );
				}

				if( !device.Hotel.HasMobileLite )
				{
					if( !device.Hotel.HotelDetail.HasSVNiPad )
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Flights,
							Icon = "flights.png",
							IsSilverlight = true,
							UseCustomNavigation = true
						} );

					menuItems.Add( new MenuItemModel
					{
						Text = device.Hotel.HotelDetail.HasSVNiPad ? ConnectCMSResources.Postcards : ConnectCMSResources.SocialPostcards,
						Icon = "postcard.png",
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}

				if( !device.Hotel.HotelDetail.HasSVNiPad )
				{
					#region Sponsorship

					var sponsership = new MenuItemModel
					{
						Text = ConnectCMSResources.SponsorshipSpace,
						Icon = "sponsorship.png",
						IsSilverlight = true,
						UseCustomNavigation = true
					};

					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessHomeScreenAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.HomeScreen,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessHotelInfoAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.HotelInfo,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessMapsDirectionsAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.MapsAndDirections,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessRecommendedAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Recommended,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessFlightsAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Flights,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessWeatherAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Weather,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
					if( user.DefaultReachRole.ViewAllAds || device.Hotel.HotelDetail.AccessPostcardAds )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Postcard,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}

					if( user.DefaultReachRole.ViewAllAds && device.DeviceDetail.HasAdBoard )
					{
						sponsership.SubMenuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.AdBoard,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}

					if( sponsership.SubMenuItems.Count > 0 && !device.Hotel.HasMobileLite )
					{
						menuItems.Add( sponsership );
					}

					#endregion Sponsorship

					if( user.DefaultReachRole.ViewAllGroups && !device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.PreviewDevice,
							Icon = "preview_device.png",
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}

					if( !device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Analytics,
							Icon = "analytics.png",
							IsSilverlight = true,
							UseCustomNavigation = true
						} );

						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.MobileAnalytics,
							Icon = "mobile_analytics.png",
							IsSilverlight = true,
							UseCustomNavigation = true
						} );

						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.RequestAnalytics,
							Icon = "request_analytics.png",
							IsSilverlight = false,
							UseCustomNavigation = true,
							Url = "Analytic/RequestAnalytics",
							WithDeviceId = true
						} );
					}

					if( device.Hotel.HotelBrandMaps.Any( m => !string.IsNullOrWhiteSpace( m.HotelBrand.HeritageUrl ) ) &&
						!device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.MobileAppMarketing,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}

					if( device.Hotel.HotelDetail.HasMobile && user.DefaultReachRole.EditConfiguration && !device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.MobileHomeScreenButtons,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}

					if( device.Hotel.HotelDetail.HasMobile && device.Hotel.HotelDetail.EnableMobileMessages &&
						!device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Messages,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}

					if( device.Hotel.HotelDetail.HasConnect && !device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.RequestGroups,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.RequestTemplates,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.RequestHandling,
							Icon = null,
							IsSilverlight = false,
							UseCustomNavigation = true,
							Url = "Request/Index"
						} );
					}
					if( device.DeviceDetail.IsChalkboardAvailable && !device.Hotel.HasMobileLite )
					{
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.Chalkboard,
							Icon = null,
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
					}
				}
				if( device.Hotel.HotelDetail.HasSVNiPad && !device.Hotel.HasMobileLite )
				{
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.Tours,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.SPG,
						Icon = null,
						IsSilverlight = true,
						UseCustomNavigation = true
					} );
				}

				if( user.DefaultReachRole.EditConfiguration )
				{
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.ConfigEditor,
						Icon = "config_editor.png",
						IsSilverlight = true,
						UseCustomNavigation = true
					} );

					if( !device.Hotel.HasMobileLite && !device.Hotel.HotelDetail.HasSVNiPad )
						menuItems.Add( new MenuItemModel
						{
							Text = ConnectCMSResources.DeviceConfig,
							Icon = "device_config.png",
							IsSilverlight = true,
							UseCustomNavigation = true
						} );
				}

				if( device.SubDevices.Any( x => x.DeviceType == DeviceType.InfoPointReaderboardTV ) )
				{
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.ConfigureReaderboards,
						IsSilverlight = false,
						UseCustomNavigation = true,
						Url = "SubDevice/ConfigureReaderboards",
						WithDeviceId = true
					} );
				}

				if( !string.IsNullOrWhiteSpace( device.Hotel.HotelDetail.SMSWelcomeMessage ) &&
					device.Hotel.HotelDetail.FKSMSWelcomeRequestType != null )
				{
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.SMSMessaging,
						IsSilverlight = false,
						UseCustomNavigation = true,
						Url = "Request/SMSMessaging",
						WithDeviceId = true
					} );
				}

				if (HotelConfigSetting.EnableCheckedInNotifications.GetHotelSetting<bool>(device.Hotel))
				{
					menuItems.Add( new MenuItemModel
					{
						Text = ConnectCMSResources.CheckedInNotifications,
						IsSilverlight = false,
						UseCustomNavigation = true,
						Url = "Notification/Jobs",
						WithDeviceId = true
					});
				}

				//// Release Notes test page
				//menuItems.Add( new MenuItemModel
				//{
				//	Text = "Release Notes",
				//	Icon = null,
				//	IsSilverlight = false,
				//	UseCustomNavigation = true,
				//	Url = "ReleaseNotes/TestNotes"
				//} );

				//// Release Notes notifications test page
				//menuItems.Add( new MenuItemModel
				//{
				//	Text = "Notifications Test",
				//	Icon = null,
				//	IsSilverlight = false,
				//	UseCustomNavigation = true,
				//	Url = "ReleaseNotes/TestNotifications"
				//} );
			}

			if( user.DefaultReachRole.Name == "Super Admin" )
			{
				var creativeServices = new MenuItemModel
				{
					Text = ConnectCMSResources.CreativeServices,
					Icon = null,
					IsSilverlight = false,
					UseCustomNavigation = true
				};
				creativeServices.SubMenuItems.Add( new MenuItemModel
				{
					Text = ConnectCMSResources.ImageUploader,
					Icon = null,
					IsSilverlight = false,
					UseCustomNavigation = true,
					Url = "CreativeService/ImageUploader"
				} );
				creativeServices.SubMenuItems.Add( new MenuItemModel
				{
					Text = ConnectCMSResources.ThemeBuilder,
					Icon = null,
					IsSilverlight = false,
					UseCustomNavigation = true,
					Url = "CreativeService/ThemeBuilder"
				} );
				menuItems.Add( creativeServices );
			}

			return menuItems;
		}

		#endregion Device

		#region Blacklist and Intrusion Log

		public bool IsResetRequestBlackListed( string clientIp )
		{
			Rp.ExecuteAction(
				   () =>
					   ( from bl in Context.LoginBlacklists
						 where bl.IsActive
						 select bl ).ForEach( bl =>
						{
							if( bl.TimeStamp < bl.ThreatLevel.ExpirationUtc() )
								bl.IsActive = false;
						} ) );

			Rp.ExecuteAction( () => Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );

			return
				Rp.ExecuteAction(
					() =>
						( from bl in Context.LoginBlacklists where bl.IsActive && bl.ClientIP == clientIp select bl )
							.FirstOrDefault() ) != null;
		}

		public bool IsResetRequestBlackListed()
		{
			return IsResetRequestBlackListed( GetClientIpAddress() );
		}

		public void LogLoginRequest( string clientIp, LoginActivityType activityType, string username, string requestGuid )
		{
			Task.Run( () =>
			{
				var localContext = new MonsciergeEntities
				{
					CommandTimeout = ( int )TimeSpan.FromMinutes( 30 ).TotalSeconds
				};

				var data =
					   XmlUtils.BuildXmlString( new Dictionary<string, object>
						{
							{"username", username == null ? null : username.ToLower()},
							{"requestguid", requestGuid == null ? null : requestGuid.ToUpper()}
						} );

				localContext.LoginActivityLogs.Add( new LoginActivityLog
				{
					ActivityType = activityType,
					ClientIP = clientIp,
					AdditionalData = data,
					TimeStamp = DateTime.UtcNow
				} );

				var expiration = DateTime.UtcNow.AddHours( -1 );
				Rp.ExecuteAction(
					() =>
						( from al in localContext.LoginActivityLogs
						  where al.IsActive && al.TimeStamp < expiration
						  select al ).ForEach( al => al.IsActive = false ) );

				Rp.ExecuteAction(
					() =>
						( from bl in localContext.LoginBlacklists
						  where bl.IsActive
						  select bl ).ForEach( bl =>
							{
								if( bl.TimeStamp < bl.ThreatLevel.ExpirationUtc() )
									bl.IsActive = false;
							} ) );

				Rp.ExecuteAction( () => localContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );

				var alCount =
					Rp.ExecuteAction(
						() => localContext.LoginActivityLogs.Count( al => al.IsActive && al.ClientIP == clientIp ) );

				if( alCount > LoginBlacklistThreatLevel.ThreatLevel1.Threshold() )
				{
					var sendRequests =
						Rp.ExecuteAction(
					() => localContext.LoginActivityLogs.Where(
							al =>
								al.IsActive && al.ClientIP == clientIp &&
								al.ActivityType == LoginActivityType.ResetRequest ).ToList() );

					var sR = Rp.ExecuteAction(
					() => sendRequests.Select( x =>
					{
						var xml = new XmlDocument();
						xml.LoadXml( x.AdditionalData );
						var dataNode = xml.SelectSingleNode( "/additional_data" );
						return new
						{
							x.ClientIP,
							x.ActivityType,
							x.TimeStamp,
							x.IsActive,
							x.UniqueId,
							Username = ( dataNode != null && dataNode.HasChildNodes && dataNode[ "username" ] != null ) ? dataNode[ "username" ].InnerText : ""
						};
					} ).ToList() );

					var redeemRequests =
						Rp.ExecuteAction(
					() => localContext.LoginActivityLogs.Where(
							al =>
								al.IsActive && al.ClientIP == clientIp &&
								al.ActivityType == LoginActivityType.ResetRedeem ).ToList() );
					var rR = Rp.ExecuteAction(
					() => redeemRequests.Select( x =>
					{
						var xml = new XmlDocument();
						xml.LoadXml( x.AdditionalData );
						var dataNode = xml.SelectSingleNode( "/additional_data" );
						return new
						{
							x.ClientIP,
							x.ActivityType,
							x.TimeStamp,
							x.IsActive,
							x.UniqueId,
							ResetGuid = ( dataNode != null && dataNode.HasChildNodes && dataNode[ "requestguid" ] != null ) ? dataNode[ "requestguid" ].InnerText : ""
						};
					} ).ToList() );

					var loginRequests =
					  Rp.ExecuteAction(
				  () => localContext.LoginActivityLogs.Where(
						  al =>
							  al.IsActive && al.ClientIP == clientIp &&
							  al.ActivityType == LoginActivityType.LoginFailed ).ToList() );
					var lR = Rp.ExecuteAction(
				   () => loginRequests.Select( x =>
				   {
					   var xml = new XmlDocument();
					   xml.LoadXml( x.AdditionalData );
					   var dataNode = xml.SelectSingleNode( "/additional_data" );
					   return new
					   {
						   x.ClientIP,
						   x.ActivityType,
						   x.TimeStamp,
						   x.IsActive,
						   x.UniqueId,
						   ResetGuid = ( dataNode != null && dataNode.HasChildNodes && dataNode[ "username" ] != null ) ? dataNode[ "username" ].InnerText : ""
					   };
				   } ).ToList() );

					var blListing =
						Rp.ExecuteAction(
					() => localContext.LoginBlacklists.FirstOrDefault( bl => bl.IsActive && bl.ClientIP == clientIp ) );

					var sendEmail = false;

					if( blListing == null )
					{
						blListing = new LoginBlacklist { ClientIP = clientIp, ThreatLevel = LoginBlacklistThreatLevel.ThreatLevel1 };
						localContext.LoginBlacklists.Add( blListing );
						sendEmail = true;
					}
					else
					{
						var allValues = ( LoginBlacklistThreatLevel[] )Enum.GetValues( typeof( LoginBlacklistThreatLevel ) );
						var threatLevel = allValues.Where( x => x.Threshold() <= alCount ).Max();
						if( blListing.ThreatLevel != threatLevel )
						{
							sendEmail = true;
							blListing.ThreatLevel = threatLevel;
						}
						blListing.TimeStamp = DateTime.UtcNow;
					}

					Rp.ExecuteAction(
					() => localContext.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );

					if( !sendEmail )
						return;

					var emailService = new EmailService();
					emailService.SendEmail(
						"monscierge@monscierge.com",
						"Jake.Donnelly@monscierge.com", //"developmentteam@monscierge.com",
						string.Format( "Potential Password Reset Attack ({0})", blListing.ThreatLevel.Description() ),
						string.Format(
							"The IP Address of {0} " +
							"\r\n " +
							"has attempted to send reset password requests for the following accounts: " +
							"\r\n " +
							"{1} " +
							"\r\n " +
							"for a total of {2} times " +
							"\r\n " +
							"and has attempted to redeem the following tokens: " +
							"\r\n " +
							"{3} " +
							"\r\n " +
							"for a total of {4} time(s)" +
							"\r\n " +
							"and has failed to sign-in to the following accounts: " +
							"\r\n " +
							"{5} " +
							"\r\n " +
							"for a total of {6} time(s)",
							clientIp,
							sR.Select( x => x.Username )
								.Distinct()
								.Aggregate( "",
									( current, next ) =>
										current +
										( !string.IsNullOrEmpty( current ) ? "\r\n" : "" ) +
										next +
										" ( " +
										sR.Count( x => x.Username == next ) +
										" )" ),
							sendRequests.Count(),
							rR
								.Select( x => x.ResetGuid )
								.Distinct()
								.Aggregate( "", ( current, next ) =>
									current +
									( !string.IsNullOrEmpty( current ) ? "\r\n" : "" ) +
									next +
									" ( " +
									rR.Count( x => x.ResetGuid == next ) +
									" )" ),
							redeemRequests.Count(),
							lR
								.Select( x => x.ResetGuid )
								.Distinct()
								.Aggregate( "", ( current, next ) =>
									current +
									( !string.IsNullOrEmpty( current ) ? "\r\n" : "" ) +
									next +
									" ( " +
									lR.Count( x => x.ResetGuid == next ) +
									" )" ),
							loginRequests.Count()
							)
						);
				}
			} );
		}

		public void LogLoginRequest( LoginActivityType activityType, string username, string requestGuid )
		{
			LogLoginRequest( GetClientIpAddress(), activityType, username, requestGuid );
		}

		#endregion Blacklist and Intrusion Log

		#region Utils

		public string GetClientIpAddress()
		{
			try
			{
				var request = HttpContext.Current.Request;

				var userHostAddress = request.UserHostAddress;

				// Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
				// Could use TryParse instead, but I wanted to catch all exceptions
				if( userHostAddress != null )
				{
					IPAddress.Parse( userHostAddress );

					var xForwardedFor = request.ServerVariables[ "X_FORWARDED_FOR" ];

					if( string.IsNullOrEmpty( xForwardedFor ) )
						return userHostAddress;

					// Get a list of public ip addresses in the X_FORWARDED_FOR variable
					var publicForwardingIps = xForwardedFor.Split( ',' ).Where( ip => !IsPrivateIpAddress( ip ) ).ToList();

					// If we found any, return the last one, otherwise return the user host address
					return publicForwardingIps.Any() ? publicForwardingIps.Last() : userHostAddress;
				}
			}
			catch( Exception )
			{
				// Always return all zeroes for any failure (my calling code expects it)
				return "0.0.0.0";
			}
			return "0.0.0.0";
		}

		private static bool IsPrivateIpAddress( string ipAddress )
		{
			// http://en.wikipedia.org/wiki/Private_network
			// Private IP Addresses are:
			//  24-bit block: 10.0.0.0 through 10.255.255.255
			//  20-bit block: 172.16.0.0 through 172.31.255.255
			//  16-bit block: 192.168.0.0 through 192.168.255.255
			//  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

			var ip = IPAddress.Parse( ipAddress );
			var octets = ip.GetAddressBytes();

			var is24BitBlock = octets[ 0 ] == 10;
			if( is24BitBlock )
				return true; // Return to prevent further processing

			var is20BitBlock = octets[ 0 ] == 172 && octets[ 1 ] >= 16 && octets[ 1 ] <= 31;
			if( is20BitBlock )
				return true; // Return to prevent further processing

			var is16BitBlock = octets[ 0 ] == 192 && octets[ 1 ] == 168;
			if( is16BitBlock )
				return true; // Return to prevent further processing

			var isLinkLocalAddress = octets[ 0 ] == 169 && octets[ 1 ] == 254;
			return isLinkLocalAddress;
		}

		private static bool IsWeakPassword( string password )
		{
			bool result;

			if( password.Length >= 8 )
			{
				var numConditionsMet = 0;
				var r = new Regex( "^(?=.*[a-z])" );
				if( r.IsMatch( password ) )
				{
					numConditionsMet++;
				}

				r = new Regex( "^(?=.*[A-Z])" );
				if( r.IsMatch( password ) )
				{
					numConditionsMet++;
				}

				r = new Regex( "^(?=.*[0-9])" );
				if( r.IsMatch( password ) )
				{
					numConditionsMet++;
				}

				r = new Regex( "^(?=.*[^a-zA-Z0-9])" );
				if( r.IsMatch( password ) )
				{
					numConditionsMet++;
				}

				if( numConditionsMet >= 3 || ( numConditionsMet >= 2 && password.Length >= 16 ) )
				{
					result = false;
				}
				else
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}

			return result;
		}

		private void GenerateSaltAndHashForUser( ContactUser user, string password )
		{
			var random = new RNGCryptoServiceProvider();
			var salt = new byte[ 32 ]; //256 bits
			random.GetBytes( salt );
			user.Salt = Convert.ToBase64String( salt );

			if( password == null ) //generate a random password if none was supplied.
			{
				var randomPwd = new byte[ 32 ];
				random.GetBytes( randomPwd );
				password = Convert.ToBase64String( randomPwd );
			}

			user.PasswordHash = GenerateHashForSaltAndPassword( user.Salt, password );
			user.IsWeakPassword = false;

			user.LastModifiedDateTime = DateTime.Now;
		}

		private string GenerateHashForSaltAndPassword( string base64Salt, string password )
		{
			var bytes = Encoding.UTF8.GetBytes( password );
			var saltBytes = Convert.FromBase64String( base64Salt );

			var allBytes = new byte[ bytes.Length + saltBytes.Length ];
			bytes.CopyTo( allBytes, 0 );
			saltBytes.CopyTo( allBytes, bytes.Length );
			var hash = new SHA256Managed();

			return Convert.ToBase64String( hash.ComputeHash( allBytes ) );
		}

		#endregion Utils

		#endregion Methods

		#region Properties

		internal string AuditLogUserId
		{
			get
			{
				if( HttpContext.Current == null || !HttpContext.Current.User.Identity.IsAuthenticated )
				{
					return "Anonymous";
				}
				return "CU|" + HttpContext.Current.User.Identity.GetUserId();
			}
		}

		private IAuthenticationManager AuthenticationManager
		{
			get { return HttpContext.Current.GetOwinContext().Authentication; }
		}

		#endregion Properties

		public void AssertEventGroupPemission( int groupId )
		{
			if( IsSuperAdmin() )
				return;
			var eventGroup = ( from eg in Context.EventGroups where eg.PKID == groupId select eg ).FirstOrDefault();
			if( eventGroup == null )
				throw new InvalidDataException( "The event group you are trying to access does not exist" );

			var user = GetLoggedInUser();

			if( user.DefaultReachRole.Name == "SuperAdmin" )
				return;
			if( user.EventGroupManagerMaps.Any( x => x.FKEventGroup == groupId ) )
				return;
			AssertDevicePermissions( eventGroup.FKDevice );
		}

		public bool IsEventManager()
		{
			var user = GetLoggedInUser();
			return user.DefaultReachRole.ManageAssignedEvents;
		}

		public bool IsBetaUser()
		{
			//3238 - Hyatt Regency Bellevue
			//117297 -- Caribe Royale
			var betaAccounts = new int?[] { 3238, 117297 };
			// 6317 - Darrique Barton - dbarton@nylohotels.com
			var betaUsers = new int?[] { 6317, 14389 };
			var betaEmailDomains = new[] { "monscierge.com", "dolce.com", "omni.com", "sheratonokc.com", "silveradoresort.com", "compass-cameat.com" };
			var user = GetLoggedInUser();
			var device = RootRepository.DeviceRepository.GetDeviceForLoggedInUser();
			return betaAccounts.Contains( user.FKAccount ) || betaUsers.Contains( user.PKID ) || ( device != null && betaAccounts.Contains( device.Hotel.FKAccount ) ) || betaEmailDomains.Any( x => user.Email.ToLower().EndsWith( x ) );
		}

		public void AssertSuperAdmin()
		{
			if( !IsSuperAdmin() )
				throw new SecurityException( "You are not authorized to access this content" );
		}

		public List<ContactUserSetting> GetContactUserSettings()
		{
			var user = GetLoggedInUser();
			if( user == null )
				throw new SecurityException( "No current logged in user" );

			return user.ContactUserSettings.ToList();
		}

		public List<ContactUserSetting> InsertOrUpdateContactUserSettings( IDictionary<string, string> contactSettings )
		{
			var user = GetLoggedInUser();
			if( user == null )
				throw new SecurityException( "No current logged in user" );

			foreach( var contactSetting in contactSettings )
			{
				var key = ( ContactUserSettingKeys )Enum.Parse( typeof( ContactUserSettingKeys ), contactSetting.Key );
				var setting = user.ContactUserSettings.FirstOrDefault( x => x.Key == ( int )key );
				if( setting == null )
					user.ContactUserSettings.Add( new ContactUserSetting { Key = ( int )key, Value = contactSetting.Value } );
				else
					setting.Value = contactSetting.Value;
			}

			Context.LogValidationFailSaveChanges( AuditLogUserId );

			return user.ContactUserSettings.ToList();
		}

		public void AssertAdBoardTagPermission( int adBoardId )
		{
			if( IsSuperAdmin() )
				return;
			var user = GetLoggedInUser();
			var res = RootRepository.WelcomeTabletRepository.CheckAdBoardPermission( user.PKID, adBoardId );
			if( res.Result != PermissionResults.Authorized )
				throw new SecurityException( "You are not authorized to access this content" );
		}

		public bool IsCampaignManager()
		{
			var user = GetLoggedInUser();
			return user.DefaultReachRole.ManageAssignedCampaigns;
		}

		public bool IsRequestUser()
		{
			var user = GetLoggedInUser();
			return user.FKRequestUser != null;
		}
	}
}