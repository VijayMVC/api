using ConnectCMS.Models.Analytic;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using MonsciergeWebUtilities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace ConnectCMS.Repositories
{
	public class AnalyticRepository : ChildRepository
	{
		public AnalyticRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public AnalyticRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp,
			ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public string GetJsonAnalyticParameters( int deviceId )
		{
			RootRepository.SecurityRepository.AssertDeviceAuthorization( deviceId );
			var device = RootRepository.DeviceRepository.GetDevice( deviceId );

			var requestCategories =
				Rp.ExecuteAction(
					() =>
						ProxylessContext.RequestCategories.Where( x => x.FKHotel == device.FKHotel )
							.OrderBy( x => x.Ordinal )
							.Select(
								x =>
									new { x.PKID, x.Name } ) ).Distinct().OrderBy( x => x.Name ).ToArray();

			var requestTypes =
				Rp.ExecuteAction(
					() =>
						ProxylessContext.RequestTypes.Where( x => x.FKHotel == device.FKHotel )
							.OrderBy( x => x.Ordinal )
							.Select( x => new { x.PKID, x.Name, x.FKRequestCategory } ) ).Distinct().OrderBy( x => x.Name ).ToArray();

			var requestUsers =
				Rp.ExecuteAction(
					() =>
						ProxylessContext.RequestGroups.Where( x => x.FKHotel == device.FKHotel )
							.SelectMany( x => x.RequestGroupUserMaps.Select( y => new { y.RequestUser.PKID, y.RequestUser.Name } ) ) )
					.Distinct().OrderBy( x => x.Name )
					.ToArray();

			return new { RequestCategories = requestCategories, RequestTypes = requestTypes, RequestUsers = requestUsers, PKID = deviceId }.ToJSON();
		}

		public List<RequestAnalyticsResult> GetRequestReport( int? deviceId, DateTime fromDate, DateTime toDate,
			int? requestCategory, int? requestType, int? requestUser, string guestName, string roomNumber )
		{
			Device device;
			if( !deviceId.HasValue )
			{
				device = RootRepository.DeviceRepository.GetAdminDeviceForLoggedInUser();
				if( device == null )
					throw new SecurityException( "You do not have analytic access to any devices" );
			}
			else
			{
				device = RootRepository.DeviceRepository.GetDevice( deviceId.Value );
			}

			//created time
			//resolution time (timespan)
			//request type eta
			//department
			//request type
			//guest name
			//room number
			//closed by
			//created by

			var requests = Context.Requests.Where(
				x => ( x.Status == RequestStatus.CLOSED || x.Status == RequestStatus.ARCHIVED ) && x.RequestType.FKHotel == device.FKHotel );

			var requestTypes =
				requests.Where(
					x => x.RequestType.IsActive && ( !requestType.HasValue || x.RequestType.PKID == requestType.Value ) );

			var requestCategories =
				requestTypes.Where(
					x =>
						( ( !requestCategory.HasValue ) ||
						 x.RequestType.FKRequestCategory == requestCategory.Value ) );

			var inSpan =
				requestCategories.Where(
					x =>
						x.RequestActions.Where( y => y.NewStatus == RequestStatus.CLOSED )
							.OrderByDescending( y => y.ActionTime )
							.FirstOrDefault()
							.ActionTime >= fromDate &&
						x.RequestActions.Where( y => y.NewStatus == RequestStatus.CLOSED )
							.OrderByDescending( y => y.ActionTime )
							.FirstOrDefault()
							.ActionTime <= toDate );

			var byUser =
				inSpan.Where(
					x =>
						!requestUser.HasValue ||
						x.RequestActions.Where( y => y.NewStatus == RequestStatus.CLOSED )
						.OrderByDescending( y => y.ActionTime )
						.FirstOrDefault().FKRequestUser == requestUser );

			var byGuest =
				byUser.Where(
					x => string.IsNullOrEmpty( guestName ) ||
						 (
							 (
								 x.FKRequesterRequestUser.HasValue
								 && x.RequesterRequestUser.Name.ToLower().Contains( guestName.ToLower() )
							)
							||
							(
								x.RequesterData != null
							)
						)
					);

			var byRoomNumber =
				byGuest.Where(
					x => string.IsNullOrEmpty( roomNumber ) ||
						 x.RoomNumber == roomNumber
					);

			var results = byRoomNumber.ToList().Select( x => new RequestAnalyticsResult
			{
				CreatedTime = x.RequestActions.First().ActionTime,
				ClosedTime = x.RequestActions.Last( y => y.NewStatus == RequestStatus.CLOSED ).ActionTime,
				Duration =
					x.RequestActions.Last( y => y.NewStatus == RequestStatus.CLOSED )
						.ActionTime.Subtract( x.RequestActions.First().ActionTime ).TotalMilliseconds,
				ETA = x.RequestType.EstimatedETASeconds,
				RequestType = x.RequestType.Name,
				RequestCategory = x.RequestType.RequestCategory != null ? x.RequestType.RequestCategory.Name : null,
				GuestName = x.FKRequesterRequestUser.HasValue ? x.RequesterRequestUser.Name : null,
				RoomNumber = x.RoomNumber,
				ClosedBy = x.RequestActions.Last( y => y.NewStatus == RequestStatus.CLOSED )
					.RequestUser.Name,
				CreatedBy = x.CreatorRequestUser.Name,
				RequesterData = x.RequesterData
			} ).ToList();

			if( !string.IsNullOrEmpty( guestName ) )
			{
				results = results.Where( x => x.GuestName.ToLower().Contains( guestName.ToLower() ) ).ToList();
			}

			return results;
		}
	}
}