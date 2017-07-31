using System;
using System.Net;
using System.Runtime.Serialization;

namespace Cllearworks.COH.Utility
{
    public class COHHttpException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool Loggable { get; set; }

        public COHHttpException(HttpStatusCode status, string message)
            : base(message)
        {
            StatusCode = status;
            Loggable = false;
        }

        public COHHttpException(HttpStatusCode status, bool loggable)
        {
            StatusCode = status;
            Loggable = loggable;
        }

        public COHHttpException(HttpStatusCode status, bool loggable, string message)
            : base(message)
        {
            StatusCode = status;
            Loggable = loggable;
        }

        public COHHttpException(HttpStatusCode status, bool loggable, string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = status;
            Loggable = loggable;
        }

        public COHHttpException(HttpStatusCode status, bool loggable, SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = status;
            Loggable = loggable;
        }
    }
}
