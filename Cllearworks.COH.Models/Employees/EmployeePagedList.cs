using System.Collections.Generic;

namespace Cllearworks.COH.Models.Employees
{
    public class EmployeePagedList 
    {
        public List<EmployeeListModel> Records { get; set; }
        public int TotalRecords { get; set; }
    }
}
