using ConnectCMS.Models.ContactUser;
using ConnectCMS.Repositories;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using MonsciergeWebUtilities.Filters;
using Newtonsoft.Json;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace ConnectCMS.Controllers
{
	[MonsciergeErrorHandler]
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class ControllerBase : Controller
	{
		private readonly ConnectCMSRepository _connectCmsRepository;

		protected ConnectCMSRepository ConnectCmsRepository
		{
			get { return _connectCmsRepository; }
		}

		public ControllerBase()
		{
			_connectCmsRepository = new ConnectCMSRepository();
		}

		protected override void OnAuthentication( AuthenticationContext filterContext )
		{
#if !DEBUG
			if( !Request.IsSecureConnection )
			{
				var ub = new UriBuilder( Request.Url ?? new Uri( "" ) ) { Scheme = "https", Port = 443 };
				if( !Request.IsLocal )
					ub.Host = "connect.monscierge.com";

				Response.Redirect( ub.ToString() );
			}
#endif

			base.OnAuthentication( filterContext );
		}

		protected ContactUserSettingModel GetContactUserSetting( ContactUserSettingKeys key )
		{
			return GetContactUserSettings( new List<ContactUserSettingKeys>()
			{
				key
			} ).FirstOrDefault();
		}

		protected List<ContactUserSettingModel> GetContactUserSettings( ICollection<ContactUserSettingKeys> keys )
		{
			List<ContactUserSettingModel> result = null;

			if( keys != null )
			{
				result = new List<ContactUserSettingModel>();

				// FH: Set default values.
				ContactUserSettingAttribute contactUserSettingAttribute;
				ContactUserSettingDataTypes contactUserSettingDataType;
				object value;

				foreach( ContactUserSettingKeys contactUserSettingKey in keys )
				{
					contactUserSettingAttribute = ContactUserSettingAttribute.GetAttribute( contactUserSettingKey );
					contactUserSettingDataType = default( ContactUserSettingDataTypes );
					value = null;

					if( contactUserSettingAttribute != null )
					{
						contactUserSettingDataType = contactUserSettingAttribute.DataType;
						value = contactUserSettingAttribute.DefaultValue;
					}

					result.Add( new ContactUserSettingModel()
					{
						ContactUserSettingKey = contactUserSettingKey,
						ContactUserSettingDataType = contactUserSettingDataType,
						Value = value
					} );
				}

				// FH: Set user's values.
				var user = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();

				if( user != null )
				{
					var contactUserSettings = user.ContactUserSettings.Where( cus => keys.Contains( ( ContactUserSettingKeys )cus.Key ) );
					ContactUserSettingModel contactUserSettingModel;

					if( contactUserSettings != null )
						foreach( ContactUserSetting contactUserSetting in contactUserSettings )
						{
							contactUserSettingModel = result.FirstOrDefault( r => r.Key == contactUserSetting.Key );

							if( contactUserSettingModel != null )
								switch( contactUserSettingModel.ContactUserSettingDataType )
								{
									case ContactUserSettingDataTypes.Number:
										contactUserSettingModel.Value = float.Parse( contactUserSetting.Value );

										break;

									case ContactUserSettingDataTypes.Text:
										contactUserSettingModel.Value = contactUserSetting.Value;

										break;

									case ContactUserSettingDataTypes.TrueOrFalse:
										contactUserSettingModel.Value = bool.Parse( contactUserSetting.Value );

										break;
								}
						}
				}
			}

			return result;
		}

		protected bool HasDevicePermissions( int deviceId )
		{
			return ConnectCmsRepository.SecurityRepository.HasDevicePermissions( deviceId );
		}

		protected JsonNetResult JsonNet( object data, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet, PreserveReferencesHandling preserveReferencesHandling = PreserveReferencesHandling.None )
		{
			return new JsonNetResult( data, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				PreserveReferencesHandling = preserveReferencesHandling
			}, behavior );
		}

		protected ActionResult RedirectToLocal( string returnUrl )
		{
			if( Url.IsLocalUrl( returnUrl ) )
			{
				return Redirect( returnUrl );
			}
			return Redirect( "/ConnectCMS/" );
		}
	}
}