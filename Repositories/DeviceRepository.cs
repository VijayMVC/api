using ConnectCMS.Models;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ConnectCMS.Repositories
{
	public class DeviceRepository : ChildRepository
	{
		public DeviceRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public DeviceRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public Device GetDevice( int deviceId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			var device = Rp.ExecuteAction( () => Context.Devices.FirstOrDefault( d => d.PKID == deviceId ) );

			return device;
		}

		public TimeZoneInfo GetDeviceTimeZone( int deviceId )
		{
			var device = GetDevice( deviceId );

			var result = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault( x => x.StandardName == device.DeviceDetail.TimeZoneId );

			return result;
		}

		public TimeZoneMap GetDeviceTimeZoneMap( int deviceId )
		{
			var device = GetDevice( deviceId );
			var result = Context.TimeZoneMaps.FirstOrDefault( x => x.WindowsName == device.DeviceDetail.TimeZoneId );
			return result;
		}

		public Device GetAutherizedDeviceByHotel( int hotelId )
		{
			var device =
				Rp.ExecuteAction(
					() =>
						( from h in ProxylessContext.Hotels
							  .Include( h => h.Devices )
							  .Where( h => h.PKID == hotelId )
						  select h ) )
							  .ToList()
							  .SelectMany( h => h.Devices )
							  .FirstOrDefault( d => RootRepository.SecurityRepository.HasDevicePermissions( d.PKID ) );

			if( device == null )
				throw new Exception( "The hotel you are looking for either doesn't exist or doesn't have a device that the user is authorized to view" );

			return device;
		}

		public Device GetAdminDeviceForLoggedInUser()
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			var devices = user.ReachUserGroupMaps.Where( x => x.FKReachRole == 1 )
					.SelectMany( x => Context.GetDevicesInReachGroup( x.FKReachGroup, false ) ).ToList();

			if( devices.Any() )
				return devices.First();

			var hotel = user.Account == null ? null : user.Account.Hotels.FirstOrDefault( x => x.Devices.Any() );
			return hotel == null ? null : hotel.Devices.FirstOrDefault();
		}

		public Device GetDeviceForLoggedInUser()
		{
			var user = RootRepository.SecurityRepository.GetLoggedInUser();
			if( user.FKDefaultReachRole == 6 && HttpContext.Current.Request.Url.Host == "localhost" )
			{
				var deviceId = Parser.GetAppSetting( "RequestHandlingDemoDeviceId", 23907 );
				var device = RootRepository.DeviceRepository.GetDevice( deviceId );
				return device;
			}
			else
			{
				var devices = Context.GetDevicesForUser( user.PKID ).ToList();

				if( devices.Any() )
					return devices.First();
			}

			var hotel = user.Account == null ? null : user.Account.Hotels.FirstOrDefault( x => x.Devices.Any() );
			return hotel == null ? null : hotel.Devices.FirstOrDefault();
		}

		public List<Device> GetDevicesForUser( int contactUserId )
		{
			var devices = Rp.ExecuteAction( () => Context.GetDevicesForUser( contactUserId )
				.Include( x => x.Hotel )
				.Include( x => x.Hotel.HotelDetail ).ToList() );
			return devices;
		}

		public PermissionResult CheckDevicePermission( int userId, int deviceId )
		{
			var device = GetDevice( deviceId );
			if( device == null )
			{
				return new PermissionResult( PermissionResults.InvalidObject,
					"The Device you are trying to access does not exist",
					new[] { new KeyValuePair<string, object>( "deviceId", deviceId ) }
					);
			}

			var hasPermission = Rp.ExecuteAction( () => Context.GetDevicesForUser( userId ).Select( ud => ud.PKID ).Contains( deviceId ) );
			if( hasPermission )
				return new PermissionResult { Result = PermissionResults.Authorized };

			return RootRepository.SecurityRepository.IsSuperAdmin()
				? new PermissionResult { Result = PermissionResults.Authorized }
				: new PermissionResult( PermissionResults.Unauthorized, "You are unauthorized to access this device.",
					new[] { new KeyValuePair<string, object>( "deviceId", deviceId ), } );
		}

		public bool HasDevicePermission( int userId, int deviceId )
		{
			return RootRepository.SecurityRepository.IsSuperAdmin() || Rp.ExecuteAction( () => ProxylessContext.GetDevicesForUser( userId ).Select( ud => ud.PKID ).Contains( deviceId ) );
		}

		public bool HasDevicePermission( int userId, Device device )
		{
			return RootRepository.SecurityRepository.IsSuperAdmin() || Rp.ExecuteAction( () => ProxylessContext.GetDevicesForUser( userId ).Select( ud => ud.PKID ).Contains( device.PKID ) );
		}

		public List<Device> GetDevicesForRequestUser(int requestUserId)
		{
			var devices =
				Context.RequestUsers
					.Where(ru => ru.PKID == requestUserId)
					.SelectMany(
						ru => ru.RequestGroupUserMaps.SelectMany(
							rgum => rgum.RequestGroup.Hotel.Devices
						)
					).Distinct().ToList();
			return devices;
		}
	}
}