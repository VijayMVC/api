using ConnectCMS.Enumerations;
using ConnectCMS.Models.Account;
using ConnectCMS.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using MonsciergeWebUtilities.Auth;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public, ApplyToStateMachine = false )]
	public class AccountController : ControllerBase
	{
		private MonsciergeUserManager _userManager;
		private static IDictionary<int, DateTime> VeriftCallLog = new Dictionary<int, DateTime>();

		public AccountController()
			: this( new MonsciergeUserManager( new MonsciergeUserStore() ) )
		{
		}

		public AccountController( MonsciergeUserManager userManager )
		{
			_userManager = userManager;
		}

		[AllowAnonymous]
		public ActionResult Login( string returnUrl, string username, LoginMessageType? messageType, string brand, string hotel )
		{
			var model = new LoginModel
			{
				UserName = username,
				MessageType = messageType,
				ReturnUrl = returnUrl,
			};

			ConnectCmsRepository.HotelBrandRepository.GetLoginTheme( model, brand, hotel );

			if( User.Identity.IsAuthenticated )
			{
				return RedirectToLocal( "" );
			}

			return View( model );
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login( LoginModel model, string returnUrl, string brand, string hotel )
		{
			if( ConnectCmsRepository.SecurityRepository.IsResetRequestBlackListed() )
			{
				model.MessageType = LoginMessageType.LoginError;
				ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.LoginFailed, model.UserName, null );
			}
			else if( ModelState.IsValid )
			{
				if( string.IsNullOrEmpty( model.UserName ) || string.IsNullOrEmpty( model.Password ) )
				{
					model.MessageType = LoginMessageType.LoginError;
				}
				else
				{
					var user = await _userManager.FindByEmailAsync( model.UserName, model.Password );
					if( user != null )
					{
						await SignInAsync( user, model.RememberMe );
						return RedirectToLocal( returnUrl );
					}
					model.MessageType = LoginMessageType.LoginError;
					ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.LoginFailed, model.UserName, null );
				}
			}

			ConnectCmsRepository.HotelBrandRepository.GetLoginTheme( model, brand, hotel );

			// If we got this far, something failed, redisplay form
			return View( model );
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult LogOff( string brand, string hotel )
		{
			HttpContext.GetOwinContext().Authentication.SignOut();
			return RedirectToLocal( "" );
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ForgotPassword( ForgotPasswordModel model )
		{
			ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.ResetRequest, model.UserName, null );

			if( !ModelState.IsValid || ConnectCmsRepository.SecurityRepository.IsResetRequestBlackListed() )
			{
				return RedirectToAction( "Login", new RouteValueDictionary { { "messageType", LoginMessageType.ResetError } } );
			}

			ConnectCmsRepository.SecurityRepository.SendResetEmail( model.UserName );
			return RedirectToAction( "Login",
				new RouteValueDictionary
					{
						{"username", model.UserName},
						{"messageType", LoginMessageType.ResetSent}
					} );
		}

		[AllowAnonymous]
		public ActionResult ResetPassword( string token )
		{
			ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.ResetRedeem, null, token );

			Guid guid;
			if( !ModelState.IsValid || !ConnectCmsRepository.SecurityRepository.IsValidResetToken( token, out guid ) || ConnectCmsRepository.SecurityRepository.IsResetRequestBlackListed() )
			{
				return RedirectToAction( "Login", new RouteValueDictionary { { "messageType", LoginMessageType.ResetError } } );
			}

			return View( new ResetPasswordModel
			{
				ResetGuid = guid,
				ResetInstructions = ConnectCMSResources.ResetPassword,
				ResetButtonText = ConnectCMSResources.SubmitPasswordReset
			} );
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ResetPassword( ResetPasswordModel model )
		{
			ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.ResetRedeem, null, model.ResetGuid.ToString( "N" ) );

			var messageType = LoginMessageType.ResetError;

			if( ModelState.IsValid || !ConnectCmsRepository.SecurityRepository.IsResetRequestBlackListed() )
			{
				if( ConnectCmsRepository.SecurityRepository.IsValidResetToken( model.ResetGuid ) )
				{
					var resetComplete = ConnectCmsRepository.SecurityRepository.ResetPassword( model.ResetGuid,
						model.Password1,
						model.Password2 );

					if( resetComplete )
						messageType = LoginMessageType.ResetSuccess;
				}
			}

			return RedirectToAction( "Login",
					 new RouteValueDictionary
					{
						{"messageType", messageType}
					} );
		}

		[AllowAnonymous]
		public ActionResult VerifyAccount( string token )
		{
			ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.ResetRedeem, null, token );

			Guid guid;
			if( !ModelState.IsValid || !ConnectCmsRepository.SecurityRepository.IsValidResetToken( token, out guid ) || ConnectCmsRepository.SecurityRepository.IsResetRequestBlackListed() )
			{
				return RedirectToAction( "Login", new RouteValueDictionary { { "messageType", LoginMessageType.VerificationError } } );
			}

			return View( "ResetPassword", new ResetPasswordModel
			{
				ResetGuid = guid,
				ResetTitle = ConnectCMSResources.VerifyAccount1,
				ResetInstructions = ConnectCMSResources.VerifyAccount2,
				ResetButtonText = ConnectCMSResources.SubmitPasswordSet
			} );
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult VerifyAccount( ResetPasswordModel model )
		{
			ConnectCmsRepository.SecurityRepository.LogLoginRequest( LoginActivityType.ResetRedeem, null, model.ResetGuid.ToString( "N" ) );

			var messageType = LoginMessageType.ResetError;

			if( ModelState.IsValid || !ConnectCmsRepository.SecurityRepository.IsResetRequestBlackListed() )
			{
				if( ConnectCmsRepository.SecurityRepository.IsValidResetToken( model.ResetGuid ) )
				{
					var resetComplete = ConnectCmsRepository.SecurityRepository.ResetPassword( model.ResetGuid,
						model.Password1,
						model.Password2 );

					if( resetComplete )
						messageType = LoginMessageType.VerificationSuccess;
				}
			}

			return RedirectToAction( "Login",
					 new RouteValueDictionary
					{
						{"messageType", messageType}
					} );
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<JsonNetResult> VerifyAuthentication()
		{
			int userId;
			if( int.TryParse( User.Identity.GetUserId(), out userId ) )
			{
				if( VeriftCallLog.ContainsKey( userId ) )
				{
					var timeSinceLast = DateTime.UtcNow.Subtract( VeriftCallLog[ userId ] );
					if( timeSinceLast < TimeSpan.FromSeconds( 2 ) )
					{
						await Task.Delay( TimeSpan.FromSeconds( 2 ) );
					}
				}
			}
			else
			{
				await Task.Delay( TimeSpan.FromSeconds( 2 ) );
			}

			if( !User.Identity.IsAuthenticated )
				return JsonNet( -1 );

			var ctx = Request.GetOwinContext();
			var user = ctx.Authentication.User;
			var claims = user.Claims;

			if( claims == null )
				return JsonNet( -1 );

			var expClaim = claims.FirstOrDefault( x => x.Type == "MonsciergeExpireUtc" );

			if( expClaim == null )
				return JsonNet( -1 );

			var expireOn = expClaim.Value;
			var currentUtc = DateTimeOffset.UtcNow;
			DateTimeOffset? expireUtc = new DateTimeOffset( long.Parse( expireOn ), TimeSpan.Zero );
			var remaining = ( expireUtc.Value - currentUtc ).TotalMilliseconds;

			if( userId != 0 )
			{
				VeriftCallLog[ userId ] = DateTime.UtcNow;
			}

			return JsonNet( remaining );
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing && _userManager != null )
			{
				_userManager.Dispose();
				_userManager = null;
			}
			base.Dispose( disposing );
		}

		private async Task SignInAsync( MonsciergeApplicationUser user, bool rememberMe )
		{
			var authenticationManager = HttpContext.GetOwinContext().Authentication;

			authenticationManager.SignOut( DefaultAuthenticationTypes.ExternalCookie );
			var identity = await _userManager.CreateIdentityAsync( user, DefaultAuthenticationTypes.ApplicationCookie );

			if( rememberMe )
			{
				var expires = DateTimeOffset.Now.AddYears( 1 );
				authenticationManager.SignIn( new AuthenticationProperties { IsPersistent = true, ExpiresUtc = expires }, identity );
			}
			else
			{
				authenticationManager.SignIn( new AuthenticationProperties { IsPersistent = false }, identity );
			}
		}

		// TODO: None of the code commented out below is used - Need to remove it at some point.
		//
		// Used for XSRF protection when adding external logins
		// private const string XsrfKey = "XsrfId";

		//private void AddErrors( IdentityResult result )
		//{
		//	foreach( var error in result.Errors )
		//	{
		//		ModelState.AddModelError( "", error );
		//	}
		//}

		//private bool HasPassword()
		//{
		//	var user = UserManager.FindById( User.Identity.GetUserId() );
		//	if( user != null )
		//	{
		//		return user.PasswordHash != null;
		//	}
		//	return false;
		//}

		//public enum ManageMessageId
		//{
		//	ChangePasswordSuccess,
		//	SetPasswordSuccess,
		//	RemoveLoginSuccess,
		//	Error
		//}

		//private class ChallengeResult : HttpUnauthorizedResult
		//{
		//	public ChallengeResult( string provider, string redirectUri, string userId = null )
		//	{
		//		LoginProvider = provider;
		//		RedirectUri = redirectUri;
		//		UserId = userId;
		//	}

		//	public string LoginProvider { get; set; }

		//	public string RedirectUri { get; set; }

		//	public string UserId { get; set; }

		//	public override void ExecuteResult( ControllerContext context )
		//	{
		//		var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
		//		if( UserId != null )
		//		{
		//			properties.Dictionary[ XsrfKey ] = UserId;
		//		}
		//		context.HttpContext.GetOwinContext().Authentication.Challenge( properties, LoginProvider );
		//	}
		//}
	}
}