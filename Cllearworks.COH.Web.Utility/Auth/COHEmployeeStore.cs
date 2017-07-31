using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository.Employees;
using Cllearworks.COH.Repository.Users;
using Cllearworks.COH.Utility;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public class COHEmployeeStore : IUserPasswordStore<COHApplicationUser, string>
    {
        private bool _disposed;
        public bool DisposeContext { get; set; }

        public IEmployeeRepository _employeeRepository { get; set; }

        public COHEmployeeStore()
        {
            DisposeContext = true;
            _employeeRepository = new EmployeeRepository();
        }

        public Task CreateAsync(COHApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(COHApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<COHApplicationUser> FindByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<COHApplicationUser> FindByNameAsync(string userName)
        {
            var user = _employeeRepository.GetEmployeeByEmail(userName);

            if (user != null && user.Client.IsActive)
            {
                return Task.FromResult(new COHApplicationUser()
                {
                    UserName = user.Email,
                    Id = user.Id.ToString(),
                    EmployeeId = user.Id.ToString(),
                    PasswordHash = user.DeviceId,
                    Status = (EmployeeStatus)user.Status,
                    ApplicationClientId = user.Client.Application.ClientId,
                    UserType = (int)UserTypes.Employee
                    //Salt = user.Salt
                });
            }
            return Task.FromResult(default(COHApplicationUser));
        }

        public Task<string> GetPasswordHashAsync(COHApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(COHApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(COHApplicationUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task UpdateAsync(COHApplicationUser user)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
