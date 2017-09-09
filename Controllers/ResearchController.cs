using Monscierge.Utilities;
using PostSharp.Extensibility;
using System.Web.Mvc;

namespace ConnectCMS.Controllers
{
	[LoggingAspect( EnableRaygun = true, AttributeTargetMemberAttributes = MulticastAttributes.Public )]
	public class ResearchController : ControllerBase
	{
		// GET: ResearchController
		public ActionResult Index()
		{
			return View();
		}
	}
}