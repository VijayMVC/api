using Cllearworks.COH.Models.Leaves;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Leaves
{
    public interface ILeaveManager
    {
        #region Mobile

        Task<LeaveModel> AddMeAsync(LeaveModel model, int employeeId);
        Task<LeaveModel> GetMeAsync(int id);
        Task<IEnumerable<LeaveModel>> QueryMeAsync(int employeeId);
        Task<LeaveModel> UpdateMeAsync(LeaveModel model);
        Task<bool> DeleteMeAsync(int id);
        Task<bool> CancelMeAsync(int id,int employeeId);

        #endregion Mobile

        Task<LeaveModel> GetAsync(int id, int clientId, int userId);
        Task<IEnumerable<LeaveModel>> QueryByEmployeeAsync(int employeeId, int clientId, int userId);
        Task<LeavePagedList> QueryAsync(int clientId, int userId, int page, int pageSize, int? placeId, int? departmentId, int? status);
        Task<bool> ApproveAsync(int id, int clientId, int userId);
        Task<bool> RejectAsync(int id, int clientId, int userId);
        Task<bool> CancelAsync(int id, int clientId, int userId);
    }
}
