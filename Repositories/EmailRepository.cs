using ConnectCMS.Models;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace ConnectCMS.Repositories
{
	public class EmailRepository : ChildRepository
	{
		private readonly TimeSpan _defaultSyncTimeout = new TimeSpan( 0, 0, 15 );
		private string _replacementEmailRecipients;
		private string _passwordResetUrlFragment;
		private string _currentLanguageIsoCode;
		private string _verifyAccounUrlFragment;

		public EmailRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
			Initialize();
		}

		public EmailRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context, MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
			Initialize();
		}

		private void Initialize()
		{
			var replacementRecipients = Monscierge.Utilities.Parser.GetAppSetting( "ReplacementEmailRecipients", "" );
			_replacementEmailRecipients = string.IsNullOrEmpty( replacementRecipients ) ? string.Empty : replacementRecipients;

			_passwordResetUrlFragment = Monscierge.Utilities.Parser.GetAppSetting( "CmsPasswordResetUrl", "https://connect.monscierge.com/ConnectCMS/Account/resetPassword?token=" );
			_verifyAccounUrlFragment = Monscierge.Utilities.Parser.GetAppSetting( "CmsPasswordResetUrl", "https://connect.monscierge.com/ConnectCMS/Account/verifyAccount?token=" );

			// TODO: Signup - If at some point there are alternate language translations for the email notifications, we can pull the user's current language setting and send them the appropriate message.
			//var currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
			//_currentLanguageIsoCode = string.IsNullOrEmpty( currentLanguage ) ? "en" : currentLanguage;
			_currentLanguageIsoCode = "en";
		}

		public string SendEventPlannerInvite( ContactUser eventPlanner, bool? newUser, string hotelName )
		{
			if( eventPlanner == null )
				throw new ArgumentNullException( "eventPlanner" );

			if( newUser.HasValue && newUser.Value )
			{
				var xmlDoc = new XDocument( new XDeclaration( "1.0", "utf-8", "true" ) );
				var root = new XElement( "Root" );
				root.Add( new XElement( "FirstName", eventPlanner.ContactUserName ) );
				root.Add( new XElement( "Email", eventPlanner.Email ) );
				root.Add( new XElement( "PasswordResetUrl", _verifyAccounUrlFragment + eventPlanner.ResetPassword ) );
				root.Add( new XElement( "Hotel", hotelName ) );
				xmlDoc.Add( root );

				var htmlBody = CreateEmailBody( xmlDoc, string.Format( @"Xsl\{0}\EventPlannerNew.xslt", _currentLanguageIsoCode ) );
				var textBody = CreateEmailBody( xmlDoc, string.Format( @"Xsl\{0}\EventPlannerNewPlainText.xslt", _currentLanguageIsoCode ) );

				var mailTo = !string.IsNullOrEmpty( _replacementEmailRecipients ) ? _replacementEmailRecipients : eventPlanner.Email;

				string errors;

				EmailService.SendHtmlEmailWithImbeddedImages( Constants.MonsciergeSalesEmailAddress, mailTo, "Welcome!", htmlBody,
					textBody, out errors );

				return errors;
			}
			else
			{
				var xmlDoc = new XDocument( new XDeclaration( "1.0", "utf-8", "true" ) );
				var root = new XElement( "Root" );
				root.Add( new XElement( "FirstName", eventPlanner.ContactUserName ) );
				root.Add( new XElement( "Email", eventPlanner.Email ) );
				root.Add( new XElement( "EventsUrl", "https://connect.monscierge.com/ConnectCMS/events" ) );
				root.Add( new XElement( "Hotel", hotelName ) );
				xmlDoc.Add( root );

				var htmlBody = CreateEmailBody( xmlDoc, string.Format( @"Xsl\{0}\EventPlannerExisting.xslt", _currentLanguageIsoCode ) );
				var textBody = CreateEmailBody( xmlDoc, string.Format( @"Xsl\{0}\EventPlannerExistingPlainText.xslt", _currentLanguageIsoCode ) );

				var mailTo = !string.IsNullOrEmpty( _replacementEmailRecipients ) ? _replacementEmailRecipients : eventPlanner.Email;

				string errors;

				EmailService.SendHtmlEmailWithImbeddedImages( Constants.MonsciergeSalesEmailAddress, mailTo, "Welcome!", htmlBody,
					textBody, out errors );

				return errors;
			}
		}

		public string SendPasswordReset( ContactUser user )
		{
			if( user == null )
				throw new ArgumentNullException( "user" );

			var xmlDoc = new XDocument( new XDeclaration( "1.0", "utf-8", "true" ) );
			var root = new XElement( "Root" );
			root.Add( new XElement( "FirstName", user.ContactUserName ) );
			root.Add( new XElement( "Email", user.Email ) );
			root.Add( new XElement( "PasswordResetUrl", _passwordResetUrlFragment + user.ResetPassword ) );
			xmlDoc.Add( root );

			var htmlBody = CreateEmailBody( xmlDoc, string.Format( @"Xsl\{0}\PasswordReset.xslt", _currentLanguageIsoCode ) );
			var textBody = CreateEmailBody( xmlDoc,
				string.Format( @"Xsl\{0}\PasswordResetPlainText.xslt", _currentLanguageIsoCode ) );

			var mailTo = !string.IsNullOrEmpty( _replacementEmailRecipients ) ? _replacementEmailRecipients : user.Email;

			string errors;

			EmailService.SendHtmlEmailWithImbeddedImages( Constants.MonsciergeSalesEmailAddress, mailTo, "Welcome!", htmlBody,
				textBody, out errors );

			return errors;
		}

		private static string CreateEmailBody( XDocument xmlDoc, string xsltFilePath )
		{
			if( xmlDoc == null )
				throw new ArgumentNullException( "xmlDoc" );

			string body;
			// Transform the XML document
			var xslTransform = new XslCompiledTransform();
			xslTransform.Load( AppDomain.CurrentDomain.BaseDirectory + xsltFilePath );

			var xslArgs = new XsltArgumentList();
			using( var writer = new StringWriter() )
			{
				xslTransform.Transform( xmlDoc.CreateReader(), xslArgs, writer );
				body = writer.ToString();
			}
			return body;
		}
	}
}