using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public class TipRepository : ChildRepository
	{
		public TipRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public TipRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public void DeleteTip( int deviceId, int tipId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			Rp.ExecuteAction( () =>
			{
				Context.InsiderTips.DeleteObject( ( from it in Context.InsiderTips
													where it.PKID == tipId
													select it ).FirstOrDefault() );

				Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
			} );
		}

		private void InsertTip( int deviceId, int userId, string tip, int? amenityId = null, int? enterpriseId = null, int? enterpriseLocationId = null )
		{
			Rp.ExecuteAction( () =>
			{
				int? hotelId = Rp.ExecuteAction( () => ( from d in Context.Devices.Where( d2 => d2.DeviceDetail.IsActive )
														 where d.PKID == deviceId
														 select d.FKHotel ) ).FirstOrDefault();

				if( hotelId.HasValue )
				{
					var now = DateTime.Now;

					var insiderTip = new InsiderTip()
					{
						FKAmenity = amenityId,
						FKContactUser = userId,
						FKEnterprise = enterpriseId,
						FKEnterpriseLocation = enterpriseLocationId,
						FKHotel = hotelId,
						LastModifiedDateTime = now,
						FKLastModifiedContactUser = userId,
						Tip = tip,
						TipDateTime = now
					};

					Context.InsiderTips.AddObject( insiderTip );
				}
			} );
		}

		public void InsertEnterpriseTip( int deviceId, int userId, string tip, int enterpriseId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			InsertTip( deviceId, userId, tip, null, enterpriseId );

			Rp.ExecuteAction( () => Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );
		}

		public void InsertEnterpriseLocationTip( int deviceId, int userId, string tip, int enterpriseId, int enterpriseLocationId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			InsertTip( deviceId, userId, tip, null, enterpriseId, enterpriseLocationId );

			Rp.ExecuteAction( () => Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );
		}

		public void UpdateTip( int deviceId, int tipId, string tip, int userId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );

			Rp.ExecuteAction( () =>
			{
				var insiderTip = ( from it in Context.InsiderTips
								   where it.PKID == tipId
								   select it ).FirstOrDefault();

				if( insiderTip != null )
				{
					insiderTip.Tip = tip;
					insiderTip.LastModifiedDateTime = DateTime.Now;
					insiderTip.FKLastModifiedContactUser = userId;

					Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId );
				}
			} );
		}
	}
}