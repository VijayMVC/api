using System;

namespace Cllearworks.COH.Models.Leaves
{
    public class LeaveModel
    {
        public int Id { get; set; }
        public LeaveTypes LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public LeaveStatus? Status { get; set; }
        public Nullable<int> ApprovedByEmployee { get; set; }
        public Nullable<int> ApprovedByUser { get; set; }
        public double TotalLeaveDays { get; set; }
    }
}
