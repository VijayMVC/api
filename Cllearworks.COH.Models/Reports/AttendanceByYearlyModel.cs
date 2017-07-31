using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Models.Reports
{
    public class AttendanceByYearlyModel
    {
        public int  EmployeeId { get; set; }
        public string Month { get; set; }
        public int WorkingDays { get; set; }
        public double  ActualWorkingHours { get; set; }
        public double TotalWorkingHours { get; set; } //this is count the (working hours * totalpresentdays)
        public int TotalLateDays { get; set; }
        public double TotalLessWorkingHours { get; set; }
        public double TotalOverTime { get; set; }
    }
}
