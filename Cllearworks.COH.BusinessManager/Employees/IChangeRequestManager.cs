using Cllearworks.COH.Models.Employees;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public interface IChangeRequestManager
    {
        #region Mobile
        Task<DeviceChangeRequestModel> AddAsync(DeviceChangeRequestModel model);
        #endregion

        Task<IEnumerable<DeviceChangeRequestModel>> QueryAsync(int clientId, int userId);
        Task<bool> ApproveAsync(int requestId, int clientId, int userId);
        Task<bool> DeleteDeviceChangeRequest(int requestId, int clientId, int userId);
    }
}
