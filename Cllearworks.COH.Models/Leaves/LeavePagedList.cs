using System.Collections.Generic;

namespace Cllearworks.COH.Models.Leaves
{
    public class LeavePagedList
    {
        public List<LeaveModel> Records { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPendingRecords { get; set; }
        public int TotalApprovedRecords { get; set; }
        public int TotalRejectedRecords { get; set; }
        public int TotalCancelledRecords { get; set; }
    }
}
