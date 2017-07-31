using Newtonsoft.Json;
using System;

namespace Cllearworks.COH.Models.Dashboard
{
    public class AttendanceTrendModel
    {
        //[JsonIgnore]
        public DateTime AttendanceTimeDt { get; set; }
        public string AttendanceTime { get; set; }
        public int Attendance { get; set; }
    }
}
