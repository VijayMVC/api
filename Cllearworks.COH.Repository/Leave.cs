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
    using System.Collections.Generic;
    
    public partial class Leave
    {
        public int Id { get; set; }
        public int LeaveType { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public int Status { get; set; }
        public Nullable<int> ApprovedByEmployee { get; set; }
        public Nullable<int> ApprovedByUser { get; set; }
    
        public virtual User User { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Employee Employee1 { get; set; }
    }
}
