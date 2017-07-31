using System;

namespace Cllearworks.COH.Models.Employees
{
    public class DeviceChangeRequestModel
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string GmcId { get; set; }
        public string ApnId { get; set; }
        public string Email { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
