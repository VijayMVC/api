using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConnectCMS.Models;
using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceBusLibrary;
using MonsciergeServiceUtilities;

namespace ConnectCMS.Repositories
{
	public interface INotificationRepository
	{
		IQueryable<NotificationJob> GetNotificationJobs(int hotelId);
		NotificationJob SaveNotificationJob(NotificationJob job, string auditUser);
		PermissionResult CheckNotificationJobPermission( int userId, int jobId );
		NotificationJob CancelNotificationJob( int userId, int jobId );
	}

	public class NotificationRepository:ChildRepository, INotificationRepository
	{
		public NotificationRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public NotificationRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IQueryable<NotificationJob> GetNotificationJobs( int hotelId )
		{
			var jobs = Rp.ExecuteAction( () => ProxylessContext.NotificationJobs.Where( nj => nj.FKHotel == hotelId ) );
			return jobs;
		}

		public NotificationJob SaveNotificationJob(NotificationJob job, string auditUser)
		{
			job.CreatedOn = DateTime.UtcNow;
			job.Status = NotificationJobStatus.SCHEDULED;
			job.StartDateTime = job.StartDateTime.ToUniversalTime();
			Context.NotificationJobs.Add(job);
			Context.LogValidationFailSaveChanges( auditUser );
			NotificationTopicSender.GetHandle().NotifyNotificationInserted();
			return job;
		}

		public PermissionResult CheckNotificationJobPermission( int userId, int jobId )
		{
			var hotelId = GetNotificationJob( jobId ).Select( x => x.FKHotel ).FirstOrDefault();
			var hasHotelPermission = RootRepository.HotelRepository.CheckHotelPermission( userId, hotelId );
			return hasHotelPermission;
		}

		public NotificationJob CancelNotificationJob( int userId, int jobId )
		{
			if( CheckNotificationJobPermission( userId, jobId ).Result != PermissionResults.Authorized )
				return null;
			var job = GetNotificationJob( jobId ).FirstOrDefault();
			
			if (job == null) return null;
			
			job.Status = NotificationJobStatus.CANCELED;
			ProxylessContext.LogValidationFailSaveChanges( string.Format( "CU|{0}", userId ) );
			NotificationTopicSender.GetHandle().NotifyNotificationInserted();
			return job;
		}

		public IQueryable<NotificationJob> GetNotificationJob( int jobId )
		{
			var job = Rp.ExecuteAction( () => ProxylessContext.NotificationJobs.Where( st => st.PKID == jobId ) );
			return job;
		}
	}
}