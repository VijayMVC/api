using AutoMapper;
using ConnectCMS.Models.ReleaseNotes;
using ConnectCMS.Repositories;
using ConnectCMS.ViewModels;
using Monscierge.Utilities;
using MonsciergeDataModel;
using MonsciergeWebUtilities.Actions;
using MonsciergeWebUtilities.Filters;
using Newtonsoft.Json;
using PostSharp.Extensibility;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	[MonsciergeAuthorize]
	public class ReleaseNotesController : ControllerBase
	{
		private readonly IReleaseNoteRepository _releaseNoteRepository;
		private readonly ContactUser _currentUser;

		public ReleaseNotesController()
		{
			_releaseNoteRepository = ConnectCmsRepository.ReleaseNoteRepository;
			_currentUser = ConnectCmsRepository.SecurityRepository.GetLoggedInUser();
		}

		public ActionResult TestNotifications()
		{
			return View();
		}

		public ActionResult TestNotes()
		{
			return View();
		}

		// GET: ReleaseNotes
		public ActionResult Notifications()
		{
			if( _currentUser == null )
			{
				return new HttpUnauthorizedResult() { };
			}
			var releaseNotes = _releaseNoteRepository.GetReleaseNotificationsForUser( _currentUser.PKID );
			var notes = releaseNotes.Select( note => new ReleaseNoteNotificationViewModel() { Id = note.PKID, Title = note.ShortTitle, Text = note.ShortDescription } ).ToList();
			return JsonNet( new { ReleaseNotes = notes }, JsonRequestBehavior.AllowGet );
		}

		[HttpPost]
		public ActionResult MarkRead( int id )
		{
			if( _currentUser == null )
			{
				return new HttpUnauthorizedResult() { };
			}

			_releaseNoteRepository.MarkReleaseNoteRead( id, _currentUser.PKID );

			return JsonNet( new { Result = "Success" } );
		}

		public JsonNetResult AllNotes()
		{
			var notes = _releaseNoteRepository.GetReleaseNotes().ToList();
			var viewModel = Mapper.Map<List<ReleaseNote>, List<ReleaseNoteModel>>( notes );

			var result = new JsonNetResult { Formatting = Formatting.Indented, Data = viewModel };
			return result;
		}
	}
}