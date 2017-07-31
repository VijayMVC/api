using Newtonsoft.Json;
using System;

namespace Cllearworks.COH.Models.Dashboard
{
    public class InTimeStatisticsModel
    {
        //[JsonIgnore]
        public DateTime AttendanceTimeDt { get; set; }
        public int ShiftId { get; set; }

        public string AttendanceTime { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}
