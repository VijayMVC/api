using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Users
{
    public interface IUsersRepository
    {
        Task<IQueryable<User>> GetUsersAsync(int clientId);
        Task<User> GetUserByIdAsync(int id);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        User GetUserByEmail(string email);
        Task<bool> ChangePasswordAsync(int userId, string newPassword, string newSalt);
    }
}
