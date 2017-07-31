using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cllearworks.COH.Models.Leaves
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaveTypes
    {
        Casual = 1,
        Sick = 2
    }
}
