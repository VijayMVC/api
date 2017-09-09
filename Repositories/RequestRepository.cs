using ConnectCMS.Models.Request;
using ConnectCMS.Repositories.Caching;
using Kent.Boogaart.KBCsv;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Twilio;

namespace ConnectCMS.Repositories
{
	public interface IRequestRepository
	{
		List<RequestType> GetRequestTypes( int hotelId );

		List<RequestGroup> GetRequestManagerGroups();

		List<SMSTask> PreviewSMSBlast( string path, int deviceId );

		string GetSMSMessageStatus( string sid, string accountSid );
	}

	public class RequestRepository : ChildRepository, IRequestRepository
	{
		private const string RequestTypeLocalizedNameElementName = "LocalizedText";
		private const string RequestTypeLocalizedNameLanguageAttributeName = "lang";
		private const string TWILIO_ACCOUNT_SID = "AC0070c74a95640817b26fc233e78865d1";
		private const string TWILIO_AUTH_TOKEN = "88898e87fe13a68919d062653984bf30";
		private const string TWILIO_REQUESTS_HANDLER_SID = "APb9ebf0e35bce976a38bb27000ecbdcf0";

		public RequestRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public RequestRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public List<RequestType> GetRequestTypes( int hotelId )
		{
			var requestTypes = Rp.ExecuteAction( () => ProxylessContext.RequestTypes
				.Include( "RequestOptions" )
				.Include( x => x.RequestCategory )
				.Where( r => r.FKHotel == hotelId && !r.IsDeleted && r.IsActive )
				.OrderBy( r => r.Ordinal ) ).ToList();

			// Replace the RequestType name with the correct localized name, if available.
			foreach( var requestType in requestTypes )
			{
				requestType.Name = Translation.GetLocalizedText( requestType.LocalizedName, requestType.Name );

				foreach( var option in requestType.RequestOptions )
				{
					option.Name = Translation.GetLocalizedText( option.LocalizedName, option.Name );
				}
			}

			return requestTypes;
		}

		public List<RequestGroup> GetRequestManagerGroups()
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			return user.RequestUser.RequestGroupUserMaps.Where( x => x.Manager ).Select( x => x.RequestGroup ).ToList();
		}

		public List<SMSTask> PreviewSMSBlast( string path, int deviceId )
		{
			var device = ProxylessContext.Devices.FirstOrDefault( x => x.PKID == deviceId );

			if( device == null )
				throw new InvalidDataException( "The device you are trying to import to does not exist" );

			RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );

			return RootRepository.SmsRepository.GetSMSTasksFromCsvBlob( path, device.FKHotel );
		}

		public string GetSMSMessageStatus( string sid, string accountSid )
		{
			var twilio = new TwilioRestClient( TWILIO_ACCOUNT_SID, TWILIO_AUTH_TOKEN, accountSid ?? TWILIO_ACCOUNT_SID );
			var message = twilio.GetMessage( sid );
			if( message == null )
				return null;

			return message.Status + "|" + ( message.RestException != null ? message.RestException.Message : "" );
		}
	}
}