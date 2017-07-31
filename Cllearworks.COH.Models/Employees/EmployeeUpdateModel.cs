using Cllearworks.COH.Models.Clients;
using Cllearworks.COH.Models.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Models.Employees
{
    public class EmployeeUpdateModel 
    {
        public int Id { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }        

        public int PlaceId { get; set; }
        public int DepartmentId { get; set; }
        public int ShiftId { get; set; }
    }
}
