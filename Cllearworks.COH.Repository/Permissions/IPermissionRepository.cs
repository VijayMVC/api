using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Permissions
{
    public interface IPermissionRepository
    {
        Task<bool> HasUserClientAccessAsync(int clientId, int userId);
        Task<User> GetUserById(int userId);
        Task<bool> IsSuperAdmin(int userId);
    }
}
