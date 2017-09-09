using ConnectCMS.Models;
using ConnectCMS.Repositories.Caching;
using LinqKit;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class BeaconRepository : ChildRepository
	{
		public BeaconRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public BeaconRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IQueryable<Beacon> GetBeacons( int hotelId )
		{
			return ProxylessContext.Beacons.Where( b => b.FKHotel == hotelId );
		}

		public IQueryable<Beacon> GetBeacon( int beaconId )
		{
			return ProxylessContext.Beacons.Where( b => b.PKID == beaconId );
		}

		public IQueryable<BeaconUserNearby> GetNearbyUsers( int beaconId, TimeSpan? activeFor = null, bool deleteInactive = false )
		{
			var activeTill = activeFor.HasValue ? DateTime.UtcNow.Subtract( activeFor.Value ) : DateTime.MinValue;

			if( deleteInactive )
			{
				var toDelete =
					ProxylessContext.BeaconUsersNearby.Where(
						x => x.FKBeacon == beaconId && x.EnteredOn < activeTill ).ToList();
				toDelete.ForEach( x => ProxylessContext.BeaconUsersNearby.Remove( x ) );
				ProxylessContext.LogValidationFailSaveChanges( string.Format( "B|{0}", beaconId ) );
			}

			return
				ProxylessContext.BeaconUsersNearby.AsExpandable().Include( bun => bun.User )
					.Where( b => b.FKBeacon == beaconId && b.EnteredOn > activeTill )
					.OrderBy( bun => bun.EnteredOn );
		}

		public PermissionResult CheckBeaconPermission( int userId, int beaconId )
		{
			var beacon = GetBeacon( beaconId ).Include( x => x.Hotel ).FirstOrDefault();
			if( beacon == null )
			{
				return new PermissionResult( PermissionResults.InvalidObject,
					"The Beacon you are trying to access does not exist",
					new[] { new KeyValuePair<string, object>( "beaconId", beaconId ) }
					);
			}

			return CheckBeaconPermission( userId, beacon );
		}

		public PermissionResult CheckBeaconPermission( int userId, Beacon beacon )
		{
			if( !beacon.FKHotel.HasValue || RootRepository.HotelRepository.CheckHotelPermission( userId, beacon.FKHotel.Value ).Result == PermissionResults.Authorized )
			{
				return new PermissionResult { Result = PermissionResults.Authorized };
			}

			return new PermissionResult( PermissionResults.Unauthorized, "You are unauthorized to access this beacon.",
				new[] { new KeyValuePair<string, object>( "userId", userId ), new KeyValuePair<string, object>( "beaconId", beacon.PKID ), } );
		}
	}
}