using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Users
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public async Task<IQueryable<User>> GetUsersAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Users.Where(u => u.ClientId == clientId && u.IsActive == true);
            });
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await Task.Run(() =>
            {
                return Context.Users.Where(u => u.Id == id).FirstOrDefault();
            });
        }

        public async Task<User> AddAsync(User user)
        {
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            return Context.Users.Find(user.Id);
        }

        public async Task<User> UpdateAsync(User user)
        {
            var data = await GetUserByIdAsync(user.Id);

            data.FirstName = user.FirstName;
            data.LastName = user.LastName;
            data.Email = user.Email;
            data.UpdatedOn = user.UpdatedOn;

            await Context.SaveChangesAsync();

            return await Context.Users.FindAsync(user.Id);
        }

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            return Context.Users.Where(u => u.Email == email).FirstOrDefault();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetUserByIdAsync(id);
            data.IsActive = false;
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword, string newSalt)
        {
            var data = await GetUserByIdAsync(userId);

            data.PasswordHash = newPassword;
            data.Salt = newSalt;
            data.UpdatedOn = DateTime.UtcNow;

            return await Context.SaveChangesAsync() > 0;
        }
    }
}
