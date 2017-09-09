using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public interface IDashboardRepository
	{
		int GetTodaysPostcardCount( int deviceId );

		int GetTodaysRequestCount( int hotelId );

		int GetCurrentRecommendationCountForDevice( int deviceId );

		int GetTomorrowsEventCount( int? deviceId );

		int GetUserInteractionTodayCount( int deviceId );

		int GetUserInteraction10DayAvgCount( int deviceId );

		List<EventDetail> GetTodaysEvents( int? deviceId );

		int GetTodaysInRoomDiningCount( int deviceId );
	}

	public class DashboardRepository : ChildRepository, IDashboardRepository
	{
		public DashboardRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{ }

		public DashboardRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{ }

		public int GetTodaysPostcardCount( int deviceId )
		{
			var count = Rp.ExecuteAction( () => Context.Postcards
				.Where( p => p.FKDevice == deviceId && p.SentDateTime >= DateTime.Today
				) ).Count();
			return count;
		}

		public int GetTodaysRequestCount( int hotelId )
		{
			var count = Rp.ExecuteAction( () => Context.Requests
				.Where( r => r.RequestGroup.Hotel.PKID == hotelId
							&& r.RequestActions.OrderBy( ra => ra.ActionTime ).FirstOrDefault().ActionTime > DateTime.Today
				) ).Count();
			return count;
		}

		public int GetCurrentRecommendationCountForDevice( int deviceId )
		{
			return Rp.ExecuteAction( () => Context.HotelBestOfEnterpriseMaps.Count( m => m.FKDevice == deviceId ) );
		}

		public int GetTomorrowsEventCount( int? deviceId )
		{
			int result;
			var startDateTime = deviceId.HasValue
				? GetLocalTimeForDevice( deviceId.Value, DateTime.Today.AddDays( 1 ) )
				: DateTime.Today.AddDays( 1 );
			var endDateTime = startDateTime.AddDays( 1 );

			if( deviceId.HasValue )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
				result =
					Rp.ExecuteAction(
						() =>
							ProxylessContext.EventDetails
								.Where( detail => detail.IsActive
												 && detail.FKDevice == deviceId
												 && ( detail.LocalStartDateTime >= startDateTime && detail.LocalStartDateTime < endDateTime )
								) ).Count();
			}
			else
			{
				var user = RootRepository.SecurityRepository.GetLoggedInUser();
				if( user.DefaultReachRole == null || ( !user.DefaultReachRole.ManageAssignedEvents && user.DefaultReachRole.Name != "Super Admin" ) )
				{
					throw new Exception( "You are not an event manager, please select a device to view events." );
				}

				result =
					Rp.ExecuteAction(
						() => ( from ed in ProxylessContext.EventDetails.Include( e => e.EventLocation )
								where ed.IsActive
									  && ed.EventGroup.EventGroupManagerMaps.Any( x => x.FKContactUser == user.PKID )
									  && ( ed.LocalStartDateTime >= startDateTime && ed.LocalStartDateTime < endDateTime )
								select ed
							).Count() );
			}

			return result;
		}

		public int GetUserInteractionTodayCount( int deviceId )
		{
			var startDateTime = GetLocalTimeForDevice( deviceId, DateTime.Today );
			return Rp.ExecuteAction(
						() => ( from t in Context.MobileClickTracks
								where t.Hotel.Devices.Any( d => d.PKID == deviceId )
								&& t.ClickDateTime > startDateTime
								select t
							).Count() );
		}

		public int GetUserInteraction10DayAvgCount( int deviceId )
		{
			var endDateTime = GetLocalTimeForDevice( deviceId, DateTime.Today );
			var startDateTime = endDateTime.AddDays( -10 );
			var count = Rp.ExecuteAction(
						() => ( from t in Context.MobileClickTracks
								where t.Hotel.Devices.Any( d => d.PKID == deviceId )
								&& t.ClickDateTime > startDateTime
								&& t.ClickDateTime < endDateTime
								select t
							).Count() );

			return count / 10;
		}

		public List<EventDetail> GetTodaysEvents( int? deviceId )
		{
			List<EventDetail> result;
			var startDateTime = deviceId.HasValue
				? GetLocalTimeForDevice( deviceId.Value, DateTime.Today )
				: DateTime.Today.AddDays( 1 );
			var endDateTime = startDateTime.AddDays( 1 );

			if( deviceId.HasValue )
			{
				RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
				result =
					Rp.ExecuteAction(
						() =>
							ProxylessContext.EventDetails.Include( e => e.EventLocation )
								.Where( detail => detail.IsActive
									&& detail.FKDevice == deviceId
									&& ( detail.LocalStartDateTime >= startDateTime && detail.LocalStartDateTime <= endDateTime )
								)
								.OrderBy( detail => detail.LocalStartDateTime )
								.ThenBy( detail => detail.Name )
								.Select( detail => detail ).ToList() );
			}
			else
			{
				var user = RootRepository.SecurityRepository.GetLoggedInUser();
				if( user.DefaultReachRole == null || ( !user.DefaultReachRole.ManageAssignedEvents && user.DefaultReachRole.Name != "Super Admin" ) )
				{
					throw new Exception( "You are not an event manager, please select a device to view events." );
				}

				result =
					Rp.ExecuteAction(
						() => ( from ed in ProxylessContext.EventDetails.Include( e => e.EventLocation )
								where ed.IsActive
									&& ed.EventGroup.EventGroupManagerMaps.Any( x => x.FKContactUser == user.PKID )
									&& ( ed.LocalStartDateTime >= startDateTime && ed.LocalStartDateTime <= endDateTime )
								select ed ) ).ToList();
			}

			result.ForEach( detail =>
			{
				detail.IsAllDay = detail.LocalStartDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) &&
					   detail.LocalEndDateTime.TimeOfDay == new TimeSpan( 0, 0, 0 ) && detail.LocalStartDateTime < detail.LocalEndDateTime;
				if( !detail.IsAllDay )
					return;
				detail.LocalStartDateTime = detail.LocalStartDateTime.Date;
				detail.LocalEndDateTime = detail.LocalEndDateTime.Date.AddDays( -1 );
			} );

			return result;
		}

		public int GetTodaysInRoomDiningCount( int deviceId )
		{
			var device =
				Rp.ExecuteAction( () => Context.Devices.Include( d => d.Hotel.HotelDetail ) ).SingleOrDefault( d => d.PKID == deviceId );
			if( device == null )
				return 0;

			var requestTypeId = device.Hotel.HotelDetail.FKRoomServiceRequestType;
			if( !requestTypeId.HasValue )
				return 0;

			var startDateTime = GetLocalTimeForDevice( device, DateTime.Today );
			return Rp.ExecuteAction( () => ( from r in Context.Requests
											 where r.FKRequestType == requestTypeId.Value
												   && r.RequestActions.OrderBy( ra => ra.ActionTime ).FirstOrDefault().ActionTime >= startDateTime
											 select r ).Count() );
		}

		private DateTime GetLocalTimeForDevice( int deviceId, DateTime dateTime )
		{
			var device = Rp.ExecuteAction( () => Context.Devices.Include( d => d.DeviceDetail ).SingleOrDefault( d => d.PKID == deviceId ) );
			return GetLocalTimeForDevice( device, dateTime );
		}

		private static DateTime GetLocalTimeForDevice( Device device, DateTime dateTime )
		{
			if( device == null || string.IsNullOrWhiteSpace( device.DeviceDetail.TimeZoneId ) )
				return dateTime;

			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( device.DeviceDetail.TimeZoneId );

			if( timeZoneInfo == null )
				return dateTime;

			var local = TimeZoneInfo.ConvertTime( dateTime, timeZoneInfo );
			return local;
		}
	}
}