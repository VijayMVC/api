using System;

namespace Cllearworks.COH.Models.Attendances
{
    public class AttendanceModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double TotalInTime { get; set; }
        public double TotalOutTime { get; set; }
        public string Remarks { get; set; }
        public bool? IsPresent { get; set; }


        public DateTime? ShiftStartTime { get; set; }
        public DateTime? ShiftEndTime { get; set; }
        public decimal? WorkingHours { get; set; }
        public decimal? BreakHours { get; set; }
    }
}
