using System.Web.Mvc;

namespace Cllearworks.COH.API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("swagger/ui/index");
        }
    }
}
