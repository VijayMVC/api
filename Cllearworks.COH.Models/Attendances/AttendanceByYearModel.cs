using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Models.Attendances
{
    public class AttendanceByYearModel
    {
        public double TotalInTime { get; set; }
        public double TotalOutTime { get; set; }
        public int TotalPresentDays { get; set; }
        public decimal? TotalRequiredWorkingHours { get; set; }        
    }
}
