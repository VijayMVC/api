using System.Web;
using System.Web.Mvc;

namespace Cllearworks.COH.Auth
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
