using System;

namespace Cllearworks.COH.Models.Reports
{
    public class AdvancedReportModel
    {
        //Employee Details
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal? WorkingHours { get; set; }
        public decimal? BreakHours { get; set; }
        public string PlaceName { get; set; }
        public string DepartmentName { get; set; }
        public string ShiftName { get; set; }
        public DateTime ShiftStartTime { get; set; }
        public DateTime ShiftEndTime { get; set; }

        //Attendance Details
        public int AttendanceId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public long? TotalInTime { get; set; }
        public long? TotalOutTime { get; set; }

        public int LateByMinutes { get; set; }
    }
}
