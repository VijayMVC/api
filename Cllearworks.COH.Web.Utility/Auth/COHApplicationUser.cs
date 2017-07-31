using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Models.Users;
using Microsoft.AspNet.Identity;
using System;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public class COHApplicationUser : IUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        public string ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public string ClientId { get; set; }

        public string EmployeeId { get; set; }

        public EmployeeStatus Status { get; set; }

        public Guid ApplicationClientId { get; set; }

        public int UserType { get; set; }
    }
}
