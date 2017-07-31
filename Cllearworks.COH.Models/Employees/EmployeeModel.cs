using Cllearworks.COH.Models.Shifts;
using System;

namespace Cllearworks.COH.Models.Employees
{
    public class EmployeeModel
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string GmcId { get; set; }
        public string ApnId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public string TotalWorkingHours { get; set; }
        public string TotalBreakHours { get; set; }
        public EmployeeStatus Status { get; set; }
        public int FailedLoginAttemptCount { get; set; }
        public DateTime TokenExpirationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int ClientId { get; set; }
        public int? PlaceId { get; set; }
        public int? DepartmentId { get; set; }

        public string ClientName { get; set; }
        public string PlaceName { get; set; }
        public string DepartmentName { get; set; }

        public int ShiftId { get; set; }
        public string EmployeeCode { get; set; }

        public string ImagePath { get; set; }

        public ShiftModel Shift { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
