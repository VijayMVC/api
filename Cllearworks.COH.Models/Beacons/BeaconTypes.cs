using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cllearworks.COH.Models.Beacons
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BeaconTypes
    {
        Entry = 0,  // Same working as Entry as well as Checkout
        CheckIn = 1,
        General = 2
    }
}
