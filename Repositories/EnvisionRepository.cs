using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class EnvisionRepository : ChildRepository
	{
		public EnvisionRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public EnvisionRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context, MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public EventDetail ImportEnvisionEvent( int deviceId, string bookingNumber, string eventAccessCode )
		{
			var device = Context.Devices.Include( x => x.Hotel.HotelDetail ).FirstOrDefault( x => x.PKID == deviceId );

			if( device == null )
				throw new DataException( "The device you are trying to import an event to does not exist" );
			RootRepository.SecurityRepository.AssertDeviceAuthorization( device.PKID );

			var data = MonsciergeServiceUtilities.EnvisionInterface.GetEventFromEnvision( device.Hotel.HotelDetail.EnvisionFacilityId, bookingNumber );
			var evts = MonsciergeServiceUtilities.EnvisionInterface.SyncronizeEvents( Context, data, device, eventAccessCode, false, false, false );

			var map =
				Context.HotelEventBookingNumberMaps.FirstOrDefault(
					x => x.FKHotel == device.FKHotel && x.BookingNumber == bookingNumber );
			if( map == null )
				Context.HotelEventBookingNumberMaps.Add( new HotelEventBookingNumberMap()
				{
					BookingNumber = bookingNumber,
					FKHotel = device.FKHotel
				} );

			Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			var firstEventTime = evts.Min( x => x.LocalStartDateTime );

			return evts.FirstOrDefault( x => x.LocalStartDateTime == firstEventTime );
		}
	}

	internal static class EventUtilities
	{
		internal static string Truncate( this string rhs, int len )
		{
			if( rhs == null || rhs.Length < len )
				return rhs;
			return rhs.Substring( 0, len );
		}
	}
}