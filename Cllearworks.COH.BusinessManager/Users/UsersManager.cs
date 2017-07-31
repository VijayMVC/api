using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Users;
using Cllearworks.COH.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Users
{
    public class UsersManager : IUsersManager
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IMappingFactory<User, UserModel, UserModel> _usersMapper;
        private readonly IMappingFactory<User, UserMeModel, UserMeModel> _userMeMapper;
        private readonly IPermissionManager _permissionManager;

        public UsersManager(IUsersRepository userRepository, IMappingFactory<User, UserModel, UserModel> usersMapper,
            IMappingFactory<User, UserMeModel, UserMeModel> userMeMapper, IPermissionManager permissionManager)
        {
            _usersRepository = userRepository;
            _usersMapper = usersMapper;
            _userMeMapper = userMeMapper;
            _permissionManager = permissionManager;
        }

        public async Task<IEnumerable<UserModel>> GetUsersAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewUser))
                throw new Exception("User has not permission to perform this operation");

            var users = await _usersRepository.GetUsersAsync(clientId);

            return users.ToList().Select(u => _usersMapper.ConvertToModel(u));
        }

        public async Task<UserModel> GetUserByIdAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewUser))
                throw new Exception("User has not permission to perform this operation");

            var user = await _usersRepository.GetUserByIdAsync(id);
            return _usersMapper.ConvertToModel(user);
        }

        public async Task<UserMeModel> GetMeAsync(int id)
        {
            var user = await _usersRepository.GetUserByIdAsync(id);
            return _userMeMapper.ConvertToModel(user);
        }

        public async Task<UserModel> AddAsync(UserModel model, string password, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddUser))
                throw new Exception("User has not permission to perform this operation");

            var existing = _usersRepository.GetUserByEmail(model.Email);

            if (existing != null)
            {
                throw new Exception("Email already exists");
                //errors = "Email already exists";
                //return new EnumerableQuery<MonsciergeDataModel.User>(new MonsciergeDataModel.User[0]);
            }

            if (!PasswordHelpers.IsValidPassword(password, new PasswordRequirements()))
            {
                throw new Exception("Password doesn't meet requirements");
                //errors = "Password doesn't meet requirements";
                //return new EnumerableQuery<MonsciergeDataModel.User>(new MonsciergeDataModel.User[0]);
            }

            var user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                ClientId = clientId,
                Role = (int)UserRoles.HRUser,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            string salt;
            string passwordHash;
            PasswordHelpers.GenerateSaltAndHash(password, out salt, out passwordHash);

            user.Salt = salt;
            user.PasswordHash = passwordHash;

            user = await _usersRepository.AddAsync(user);

            return _usersMapper.ConvertToModel(user);
        }

        public async Task<UserModel> UpdateAsync(UserModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditUser))
                throw new Exception("User has not permission to perform this operation");

            var existing = _usersRepository.GetUserByEmail(model.Email);

            if (existing != null && existing.Id != model.Id)
            {
                throw new Exception("Email already exists");
            }

            var user = _usersMapper.ConvertToDataModel(model);
            user.UpdatedOn = DateTime.UtcNow;

            user = await _usersRepository.UpdateAsync(user);

            return _usersMapper.ConvertToModel(user);
        }

        public async Task<bool> DeleteAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteUser))
                throw new Exception("User has not permission to perform this operation");

            return await _usersRepository.DeleteAsync(id);
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword, int userId)
        {

            var existing = await _usersRepository.GetUserByIdAsync(userId);

            if (existing == null)
            {
                throw new Exception("User does not exists");
            }

            if (PasswordHelpers.GenerateHashForSaltAndPassword(existing.Salt, oldPassword) != existing.PasswordHash)
            {
                throw new Exception("Old password is not valid");
            }

            if (!PasswordHelpers.IsValidPassword(newPassword, new PasswordRequirements()))
            {
                throw new Exception("Password doesn't meet requirements");
            }

            string salt;
            string passwordHash;
            PasswordHelpers.GenerateSaltAndHash(newPassword, out salt, out passwordHash);

            return await _usersRepository.ChangePasswordAsync(userId, passwordHash, salt);
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = _usersRepository.GetUserByEmail(email);

            if (user == null || !user.IsActive)
            {
                throw new Exception("User does not exists");
            }

            var newPassword = Guid.NewGuid().ToString().Substring(1, 6);

            string salt;
            string passwordHash;
            PasswordHelpers.GenerateSaltAndHash(newPassword, out salt, out passwordHash);

            await _usersRepository.ChangePasswordAsync(user.Id, passwordHash, salt);

            var message = "Hi " + user.FirstName + " " + user.LastName + ",<br><br>" +
                          "Your username is <b>" + user.Email + "</b> and new password is <b>" + newPassword + "</b>." +
                          "<br><br><br>Thanks<br>COH Team";

            EmailService.SendEmail("coh-noreply@cllearworks.com", user.Email, "Reset password for COH", message);

            return true;
        }

        public async Task<bool> SendEmailAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteUser))
                throw new Exception("User has not permission to perform this operation");

            var user = await _usersRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                throw new Exception("User does not exists");
            }

            var newPassword = Guid.NewGuid().ToString().Substring(1, 6);

            string salt;
            string passwordHash;
            PasswordHelpers.GenerateSaltAndHash(newPassword, out salt, out passwordHash);

            await _usersRepository.ChangePasswordAsync(user.Id, passwordHash, salt);

            var message = "Hi " + user.FirstName + " " + user.LastName + ",<br><br>" +
                          "Your username is <b>" + user.Email + "</b> and new password is <b>" + newPassword + "</b>." +
                          "<br><br><br>Thanks<br>COH Team";

            EmailService.SendEmail("coh-noreply@cllearworks.com", user.Email, "COH Credential Info", message);

            return true;
        }
    }
}
