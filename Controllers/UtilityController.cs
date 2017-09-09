using ConnectCMS.Models.ContactUser;
using ConnectCMS.Models.Hotel;
using ConnectCMS.Models.Utility;
using ConnectCMS.Resources;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using MonsciergeWebUtilities.Actions;
using MonsciergeWebUtilities.Models.Utility;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class UtilityController : ControllerBase
	{
		// POST: /Utility/GetBingMapCredentials
		[HttpPost]
		public JsonNetResult GetBingMapCredentials()
		{
			var bingMapCredentials = ConfigurationManager.AppSettings.Get( "BingMapCredentials" );

			return JsonNet( bingMapCredentials );
		}

		// POST: /Utility/GetCountries
		[AllowAnonymous]
		[HttpPost]
		public JsonNetResult GetCountries()
		{
			var currentCultureName = CultureInfo.CurrentCulture.Name;

			var regionInfo = !string.IsNullOrEmpty(currentCultureName) ? new RegionInfo( currentCultureName ) : null;
			var countries = ConnectCmsRepository.UtilityRepository.GetCountries().OrderBy( c => ( c.ISOCountryCode != null && (regionInfo != null ? regionInfo.TwoLetterISORegionName : "US") == c.ISOCountryCode.Trim() ) ? 0 : 1 ).ToList();

			return JsonNet( countries );
		}

		// POST: /Utility/GetCountriesWithPhoneNumber
		[HttpPost]
		public JsonNetResult GetCountriesWithPhoneNumber()
		{
			var countries = ConnectCmsRepository.UtilityRepository.GetCountriesWithPhoneNumber();

			return JsonNet( countries ?? new List<Country>() );
		}

		// POST: /Utility/GetHotelFromDevice
		[HttpPost]
		public JsonNetResult GetHotelFromDevice( int deviceId )
		{
			var hotel = ConnectCmsRepository.HotelRepository.GetHotelFromDevice( deviceId );

			return JsonNet( hotel ?? new HotelModel() );
		}

		// POST: /Utility/GetLoggedInUserId
		[HttpPost]
		public JsonNetResult GetLoggedInUserId()
		{
			int userId = ConnectCmsRepository.SecurityRepository.GetLoggedInUser().PKID;

			return JsonNet( userId );
		}

		// POST: /Utility/GetPostalCode
		[HttpPost]
		public JsonNetResult GetPostalCode( string name )
		{
			var postalCode = ConnectCmsRepository.UtilityRepository.GetPostalCodes( name ).FirstOrDefault();

			return JsonNet( postalCode ?? new PostalCode() );
		}

		// POST: /Utility/GetPostalCodes
		[HttpPost]
		public JsonNetResult GetPostalCodes( string name )
		{
			var postalCodes = ConnectCmsRepository.UtilityRepository.GetPostalCodes( name );

			return JsonNet( postalCodes ?? new List<PostalCode>() );
		}

		// POST: /Utility/GetStates
		[HttpPost]
		public JsonNetResult GetStates( int? countryId = null )
		{
			var states = ConnectCmsRepository.UtilityRepository.GetStates( countryId );

			return JsonNet( states ?? new List<State>() );
		}

		// POST: /Utility/InsertOrUpdateContactUserSetting
		[HttpPost]
		public JsonNetResult InsertOrUpdateContactUserSetting( ContactUserSettingKeys key, string value )
		{
			var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();

			if( user != null )
				ConnectCmsRepository.SecurityRepository.InsertOrUpdateContactUserSetting( user.PKID, key, value );

			return JsonNet( true );
		}

		public JsonNetResult GetContactUserSettings()
		{
			var settings = ConnectCmsRepository.SecurityRepository.GetContactUserSettings();

			return JsonNet( settings.ToDictionary( x => ( ContactUserSettingKeys )x.Key, x => x.Value ), JsonRequestBehavior.AllowGet );
		}

		[HttpPost]
		public JsonNetResult InsertOrUpdateContactUserSettings( IDictionary<string, string> contactSettings )
		{
			var settings = ConnectCmsRepository.SecurityRepository.InsertOrUpdateContactUserSettings( contactSettings );

			return JsonNet( settings.ToDictionary( x => ( ContactUserSettingKeys )x.Key, x => x.Value ) );
		}

		[AllowAnonymous]
		[HttpPost]
		public JsonNetResult SetCulture( CultureViewModel newCulture )
		{
			return SetCultureWithString( newCulture.Culture );
		}

		[AllowAnonymous]
		[HttpPost]
		public JsonNetResult SetCultureWithString( string culture )
		{
			try
			{
				var ci = new CultureInfo( culture );
				var countryCookie = Response.Cookies[ "lang" ];
				if( countryCookie != null )
				{
					countryCookie.Value = ci.Name;
					countryCookie.Expires = DateTime.MaxValue;
					Response.Cookies.Set( countryCookie );
				}
				else
				{
					countryCookie = new HttpCookie( "lang", ci.Name );
					countryCookie.Expires = DateTime.MaxValue;
					Response.Cookies.Add( countryCookie );
				}
			}
			catch( Exception )
			{
			}
			return JsonNet( true );
		}

		[AllowAnonymous]
		[HttpPost]
		public JsonNetResult GetSupportedCultures()
		{
			var language = MvcApplication.GetCurrentCulture();
			var cultures = new List<CultureViewModel>
	        {
                new CultureViewModel(){ Key = "EN", Culture = "en-US", Name = @ConnectCMSResources.English, NativeName = "English", IsActive = true},
                new CultureViewModel(){ Key = "FR", Culture = "fr-FR", Name = @ConnectCMSResources.French, NativeName = "Français", IsActive = true}
	        };
			return JsonNet( cultures.OrderByDescending( x => x.Key == language ) );
		}

		[AllowAnonymous]
		[HttpPost]
		public JsonNetResult SendFeedback( FeedbackModel feedback )
		{
			var toAddress = "ProductFeedback@Monscierge.com";
			if( feedback.FeedbackType == FeedbackType.BugReport || feedback.FeedbackType == FeedbackType.SupportRequest )
				toAddress = "Support@monscierge.com";

			//TODO REMOVE AFTER TESTING
			toAddress = "Jake.Donnelly@monscierge.com";

			var errors = "";
			var message = string.Format( "Name: {0}<br/>" +
									  "Email Address: {1}<br/>" +
									  "Subject: {2}<br/>" +
									  "Feedback Type: {3}<br/>" +
									  "Comments: {4}<br/>", feedback.Name, feedback.EmailAddress, feedback.Subject, feedback.FeedbackType, feedback.Comments );
			var messageStr = string.Format( "Name: {0}\r\n" +
										"Email Address: {1}\r\n" +
										"Subject: {2}\r\n" +
										"Feedback Type: {3}\r\n" +
										"Comments: {4}\r\n", feedback.Name, feedback.EmailAddress, feedback.Subject, feedback.FeedbackType, feedback.Comments );

			EmailService.SendHtmlEmail( "CMS.Feedback@monscierge.com", toAddress,
				string.Format( "ConnectCMS Feedback ({0}) - {1}", feedback.FeedbackType, feedback.Subject ), message,
				messageStr, out errors );

			return JsonNet( errors ?? "" );
		}

		[HttpPost]
		public JsonNetResult GetDeviceTimeZone( int deviceId )
		{
			var timezone = ConnectCmsRepository.DeviceRepository.GetDeviceTimeZoneMap( deviceId );
			if( timezone == null )
				return JsonNet( "UtC" );
			return JsonNet( timezone.OlsonName );
		}
	}
}