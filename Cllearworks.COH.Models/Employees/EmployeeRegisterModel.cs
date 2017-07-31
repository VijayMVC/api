using System;

namespace Cllearworks.COH.Models.Employees
{
    public class EmployeeRegisterModel
    {
        public string DeviceId { get; set; }
        public string GmcId { get; set; }
        public string ApnId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public Guid ApplicationClientId { get; set; }
    }
}
