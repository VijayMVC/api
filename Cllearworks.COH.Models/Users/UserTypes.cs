using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cllearworks.COH.Models.Users
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserTypes
    {
        Employee = 1,
        CMSUser = 2
    }
}
