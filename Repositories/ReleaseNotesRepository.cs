using ConnectCMS.Repositories.Caching;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using MonsciergeDataModel;
using MonsciergeServiceUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectCMS.Repositories
{
	public interface IReleaseNoteRepository
	{
		IEnumerable<ReleaseNote> GetReleaseNotificationsForUser( int contactUserId );

		void MarkReleaseNoteRead( int releaseNoteId, int contactUserId );

		IQueryable<ReleaseNote> GetReleaseNotes();
	}

	public class ReleaseNotesRepository : ChildRepository, IReleaseNoteRepository
	{
		public ReleaseNotesRepository( ConnectCMSRepository rootRepository )
			: base( rootRepository )
		{
		}

		public ReleaseNotesRepository( ConnectCMSRepository rootRepository, MonsciergeEntities context,
			MonsciergeEntities proxylessContext, RetryPolicy<SqlAzureTransientErrorDetectionStrategyEnhanced> rp, ICacheManager cacheManager )
			: base( rootRepository, context, proxylessContext, rp, cacheManager )
		{
		}

		public IEnumerable<ReleaseNote> GetReleaseNotificationsForUser( int contactUserId )
		{
			var releaseNotes = Rp.ExecuteAction( () => ( from r in Context.ReleaseNotes
														 where r.ReleaseNoteContactUserReadMaps.All( m => m.FKContactUser != contactUserId )
														 && r.NoteExpires > DateTime.Now
														 orderby r.ReleaseDate descending
														 select r ) ).ToList();

			return releaseNotes;
		}

		public void MarkReleaseNoteRead( int releaseNoteId, int contactUserId )
		{
			var readMap = Context.ReleaseNoteContactUserReadMaps.FirstOrDefault( m => m.FKReleaseNote == releaseNoteId && m.FKContactUser == contactUserId );
			if( readMap == null )
			{
				readMap = new ReleaseNoteContactUserReadMap()
				{
					FKReleaseNote = releaseNoteId,
					FKContactUser = contactUserId,
					ReadDateTime = DateTime.Now
				};
				Context.ReleaseNoteContactUserReadMaps.Add( readMap );
			}
			else
			{
				readMap.ReadDateTime = DateTime.Now;
			}

			Rp.ExecuteAction( () => Context.LogValidationFailSaveChanges( RootRepository.SecurityRepository.AuditLogUserId ) );
		}

		public IQueryable<ReleaseNote> GetReleaseNotes()
		{
			return Context.ReleaseNotes.OrderByDescending( r => r.ReleaseDate );
		}
	}
}