using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Permissions
{
    public interface IPermissionManager
    {
        Task<bool> HasUserClientAccessAsync(int clientId, int userId);
        Task<bool> HasPermission(int clientId, int userId, string permission);
        Task<bool> IsSuperAdmin(int userId);
    }
}
