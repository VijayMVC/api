using System.Net;

namespace Cllearworks.COH.Web.Utility.Exceptions
{
    public class ErrorMessageResult
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
