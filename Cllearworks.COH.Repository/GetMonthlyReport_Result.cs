//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cllearworks.COH.Repository
{
    using System;
    
    public partial class GetMonthlyReport_Result
    {
        public Nullable<System.DateTime> ShiftDate { get; set; }
        public Nullable<bool> IsWorkingDay { get; set; }
        public Nullable<int> DayOfWeek { get; set; }
        public Nullable<System.DateTime> ShiftStartTime { get; set; }
        public Nullable<System.DateTime> ShiftEndTime { get; set; }
        public Nullable<decimal> WorkingHours { get; set; }
        public Nullable<decimal> BreakHours { get; set; }
        public Nullable<int> AttendanceId { get; set; }
        public int IsPresent { get; set; }
        public Nullable<System.DateTime> AttendanceDate { get; set; }
        public Nullable<System.DateTime> CheckInTime { get; set; }
        public Nullable<System.DateTime> CheckOutTime { get; set; }
        public Nullable<long> TotalInTime { get; set; }
        public Nullable<long> TotalOutTime { get; set; }
        public Nullable<int> LateByMinutes { get; set; }
        public int IsLeave { get; set; }
        public Nullable<int> LeaveType { get; set; }
        public Nullable<int> LeaveStatus { get; set; }
        public string LeaveReason { get; set; }
        public int IsHoliday { get; set; }
        public string HolidayName { get; set; }
    }
}