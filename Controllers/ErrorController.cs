using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	public class ErrorController : Controller
	{
		// GET: Error
		public ActionResult Index( string header, string message )
		{
			ViewBag.Message = message;
			ViewBag.Header = header;
			return View();
		}
	}
}