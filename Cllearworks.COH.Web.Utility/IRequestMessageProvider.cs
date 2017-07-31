using System.Net.Http;

namespace Cllearworks.COH.Web.Utility
{
    public interface IRequestMessageProvider
    {
        HttpRequestMessage CurrentMessage { get; }
    }
}
