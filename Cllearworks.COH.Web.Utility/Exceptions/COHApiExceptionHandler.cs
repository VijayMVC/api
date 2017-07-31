using Cllearworks.COH.Utility;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace Cllearworks.COH.Web.Utility.Exceptions
{
    /*
	 * This custom exception handler class is used to intercept and handle any unhandled exceptions generated through
	 * the Monscierge API. This gives us the ability to pick off exception types in one place and send customized error
	 * messages from the API down to the consuming client. The final 'else' guarantees that the API never returns "raw"
	 * .Net error messages or stack traces to the client.
	 */

    public class COHApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            var ex = context.Exception as COHHttpException;
            if (ex != null)
            {
                var errorMessageResultCOH = new ErrorMessageResult()
                {
                    HttpStatusCode = ex.StatusCode,
                    ErrorMessage = context.Exception.Message
                };

                context.Result = new ResponseMessageResult(context.Request.CreateResponse(ex.StatusCode, errorMessageResultCOH));

                return;
            }

            //var message = context.Exception.Message;
            //message += GetInnerExceptionMessages(ref message, context.Exception.InnerException);

            HttpException httpException = context.Exception as HttpException;

            if (httpException == null)
                httpException = new HttpException(500, "Internal Server Error", context.Exception);

            var errorMessageResult = new ErrorMessageResult()
            {
                HttpStatusCode = (HttpStatusCode)httpException.GetHttpCode(),
                ErrorMessage = context.Exception.Message
            };
            context.Result = new ResponseMessageResult(context.Request.CreateResponse(errorMessageResult.HttpStatusCode, errorMessageResult));
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }

        public string GetInnerExceptionMessages(ref string message, Exception ex)
        {
            if (ex != null && ex.InnerException != null)
            {
                message += " | (InnerException): " + GetInnerExceptionMessages(ref message, ex.InnerException);
            }
            return message;
        }
    }
}
