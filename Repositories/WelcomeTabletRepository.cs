using ConnectCMS.Models;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class WelcomeTabletRepository : ChildRepository
	{
		public WelcomeTabletRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public WelcomeTabletRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IQueryable<AdBoard> GetAdBoard( int adBoardId )
		{
			return ProxylessContext.AdBoards.Where( x => x.PKID == adBoardId );
		}

		public IQueryable<AdBoard> GetAdBoard( int userId, int adBoardId )
		{
			return
				ProxylessContext.AdBoards.Where(
					x => x.PKID == adBoardId && ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
						urm =>
							urm.UserRolePermissionTagMaps.All(
								urptm => x.Hotel.PermissionHotelTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
								)
						)
					);
		}

		public IQueryable<Hotel> GetAdBoardHotels( int userId )
		{
			return Rp.ExecuteAction( () => ProxylessContext.Hotels
				.Include( x => x.PermissionHotelTagMaps.Select( y => y.PermissionTag ) )
				.Include( x => x.AdBoards )
				.Include( x => x.AdBoards.Select( ab => ab.SubDevices ) )
				.Where(
					x =>
						x.AdBoards.Any() &&
						ProxylessContext.ContactUsers.FirstOrDefault( u => u.PKID == userId ).UserRoleMaps.Any(
							urm =>
								urm.UserRolePermissionTagMaps.All(
									urptm => x.PermissionHotelTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag )
									)
							)
				)
				);
		}

		public PermissionResult CheckAdBoardPermission( int userId, int adBoardId )
		{
			var user =
				RootRepository.SecurityRepository.GetLoggedInUser();

			var adBoard = GetAdBoard( adBoardId ).Include( x => x.Hotel.PermissionHotelTagMaps ).FirstOrDefault();

			if( adBoard == null )
			{
				return new PermissionResult( PermissionResults.InvalidObject,
					"The AdBoard you are trying to access does not exist",
					new[] { new KeyValuePair<string, object>( "adBoardId", adBoardId ) }
					);
			}
			return CheckAdBoardPermission( user, adBoard );
		}

		public PermissionResult CheckAdBoardPermission( ContactUser user, int adBoardId )
		{
			var adBoard = GetAdBoard( adBoardId ).Include( x => x.Hotel.PermissionHotelTagMaps ).FirstOrDefault();

			if( adBoard == null )
			{
				return new PermissionResult( PermissionResults.InvalidObject,
					"The AdBoard you are trying to access does not exist",
					new[] { new KeyValuePair<string, object>( "adBoardId", adBoardId ) }
					);
			}
			return CheckAdBoardPermission( user, adBoard );
		}

		public PermissionResult CheckAdBoardPermission( ContactUser user, AdBoard adBoard )
		{
			if( user.UserRoleMaps.Any(
				urm =>
					urm.UserRolePermissionTagMaps.All(
						urptm => adBoard.Hotel.PermissionHotelTagMaps.Any( phtml => phtml.FKPermissionTag == urptm.FKPermissionTag ) ) ) )
				return new PermissionResult() { Result = PermissionResults.Authorized };

			return new PermissionResult( PermissionResults.Unauthorized, "You are unauthorized to access this AdBoard.",
				new[] { new KeyValuePair<string, object>( "userId", user.PKID ), new KeyValuePair<string, object>( "adBoard", adBoard.PKID ), } );
		}

		public string GetWelcomeTabletUrl( int subDeviceId )
		{
			return string.Empty;
		}

		public IEnumerable<AdBoard> GetAdBoardsUsingCampaign( int pkid )
		{
			return Context.AdBoards.Where( x => x.FKMarketingCampaign == pkid );
		}

		public IEnumerable<AdBoard> GetAdBoardsUsingCampaignScreen( int pkid )
		{
			return
				Context.AdBoards.Where(
					x =>
						x.MarketingCampaign.FKDefaultScreen == pkid ||
						x.MarketingCampaign.MarketingCampaignExceptions.Any( y => y.FKMarketingCampaignScreen == pkid ) );
		}
	}
}