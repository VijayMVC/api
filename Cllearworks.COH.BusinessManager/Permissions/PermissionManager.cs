using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Permissions
{
    public class PermissionManager : IPermissionManager
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionManager(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<bool> HasUserClientAccessAsync(int clientId, int userId)
        {
            return await _permissionRepository.HasUserClientAccessAsync(clientId, userId);
        }

        public async Task<bool> HasPermission(int clientId, int userId, string permission)
        {
            var user = await _permissionRepository.GetUserById(userId);
            if (user != null)
            {
                var isClientAccess = false;

                // Client access permission check
                if (user.Role == (int)UserRoles.SuperAdmin)
                {
                    isClientAccess = true;
                }
                if (user.ClientId == clientId)
                {
                    isClientAccess = true;
                }

                // Operation access permission check
                if (isClientAccess)
                {
                    Dictionary<string, bool> permissions = Permission.GetPermissions((UserRoles)user.Role);
                    return permissions.Any(p => p.Key == permission && p.Value == true);
                }

                return false;
            }
            return false;
        }

        public async Task<bool> IsSuperAdmin(int userId)
        {
            return await _permissionRepository.IsSuperAdmin(userId);
        }
    }
}
