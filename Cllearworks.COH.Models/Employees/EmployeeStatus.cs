using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cllearworks.COH.Models.Employees
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EmployeeStatus
    {
        Active = 1,
        InActive = 2,
        Pending = 3,
        Rejected = 4
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChangeRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
