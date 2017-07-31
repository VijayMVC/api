using Cllearworks.COH.Models.Users;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Permissions
{
    public class PermissionRepository : BaseRepository, IPermissionRepository
    {
        public async Task<bool> HasUserClientAccessAsync(int clientId, int userId)
        {
            return await Task.Run(() => 
            {
                var user = Context.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    if (user.Role == (int)UserRoles.SuperAdmin)
                    {
                        return true;
                    }
                    if (user.ClientId == clientId)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            });
        }

        public async Task<User> GetUserById(int userId)
        {
            return await Context.Users.Where(u => u.Id == userId && u.IsActive).FirstOrDefaultAsync();
        }

        public async Task<bool> IsSuperAdmin(int userId)
        {
            return await Task.Run(() =>
            {
                return Context.Users.Any(u => u.Id == userId && u.Role == (int)UserRoles.SuperAdmin);
            });
        }
    }
}
