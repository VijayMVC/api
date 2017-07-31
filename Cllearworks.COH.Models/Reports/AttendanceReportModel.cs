using Cllearworks.COH.Models.Leaves;
using System;

namespace Cllearworks.COH.Models.Reports
{
    public class AttendanceByEmployeeReportModel
    {
        public DateTime ShiftDate { get; set; }
        public bool IsWorkingDay { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime? ShiftStartDate { get; set; }
        public DateTime? ShiftEndDate { get; set; }
        public decimal? WorkingHours { get; set; }
        public decimal? BreakHours { get; set; }


        public int? AttendanceId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double? TotalInTime { get; set; }
        public double? TotalOutTime { get; set; }
        public int? LateByMinutes { get; set; }

        public bool IsLeave { get; set; }
        public LeaveTypes? LeaveType { get; set; }
        public LeaveStatus? LeaveStatus { get; set; }
        public string LeaveReason { get; set; }

        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }
    }
}
