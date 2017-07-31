using Cllearworks.COH.Models.Shifts;

namespace Cllearworks.COH.Models.Employees
{
    public class EmployeeListModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }        
        public decimal WorkingHours { get; set; }
        public decimal BreakHours { get; set; }
        public string EmployeeCode { get; set; }   
        public EmployeeStatus Status { get; set; }
        public int? PlaceId { get; set; }
        public string PlaceName { get; set; }

        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public ShiftModel Shift { get; set; }

        public string ImagePath { get; set; }
    }
}
