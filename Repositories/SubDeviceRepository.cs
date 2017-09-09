using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ConnectCMS.Repositories
{
	public class SubDeviceRepository : ChildRepository
	{
		public SubDeviceRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public SubDeviceRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context, MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IQueryable<SubDevice> GetReaderboards( int deviceId )
		{
			RootRepository.SecurityRepository.AssertDevicePermissions( deviceId );
			var readerBoards =
				Rp.ExecuteAction(
					() =>
						ProxylessContext.SubDevices.Where( x => x.DeviceType == DeviceType.InfoPointReaderboardTV && x.FKDevice == deviceId )
							.Include( x => x.ReaderboardBackgroundImage ) );

			return readerBoards;
		}
	}
}