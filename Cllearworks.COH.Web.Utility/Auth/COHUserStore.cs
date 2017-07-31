using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository.Users;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public class COHUserStore : IUserPasswordStore<COHApplicationUser, string>
    {
        private bool _disposed;
        public bool DisposeContext { get; set; }

        public IUsersRepository _userRepository { get; set; }

        public COHUserStore()
        {
            DisposeContext = true;
            _userRepository = new UsersRepository();
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
            var user = _userRepository.GetUserByEmail(userName);
            if (user != null && user.IsActive && user.Client.IsActive)
            {
                return Task.FromResult(new COHApplicationUser() {
                    UserName = user.Email,
                    Id = user.Id.ToString(),
                    PasswordHash = user.PasswordHash,
                    Salt = user.Salt,
                    UserType = (int)UserTypes.CMSUser
                });
            }
            return Task.FromResult(new COHApplicationUser());
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
