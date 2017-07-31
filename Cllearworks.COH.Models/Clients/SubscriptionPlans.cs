using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cllearworks.COH.Models.Clients
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionPlans
    {
        SubscriptionPlan1 = 1,
        SubscriptionPlan2 = 2,
        SubscriptionPlan3 = 3,
    }
}
