using Cllearworks.COH.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Users
{
    public interface IUsersManager
    {
        Task<IEnumerable<UserModel>> GetUsersAsync(int clientId, int userId);
        Task<UserModel> GetUserByIdAsync(int id, int clientId, int userId);
        Task<UserMeModel> GetMeAsync(int id);
        Task<UserModel> AddAsync(UserModel model, string password, int clientId, int userId);
        Task<UserModel> UpdateAsync(UserModel model, int clientId, int userId);
        Task<bool> DeleteAsync(int id, int clientId, int userId);
        Task<bool> ChangePasswordAsync(string oldPassword, string newPassword, int userId);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> SendEmailAsync(int id, int clientId, int userId);
    }
}
