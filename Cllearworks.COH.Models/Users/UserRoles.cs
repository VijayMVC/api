using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cllearworks.COH.Models.Users
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRoles
    {
        SuperAdmin = 1,
        ClientAdmin = 2,
        HRUser = 3
    }
}
