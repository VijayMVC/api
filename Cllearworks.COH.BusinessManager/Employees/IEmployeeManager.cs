using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public interface IEmployeeManager
    {
        #region Mobile

        Task<EmployeeModel> Login(string deviceId, string emailId);

        Task<EmployeeModel> GetMeAsync(int id);

        Task<EmployeeRegisterModel> RegisterAsync(EmployeeRegisterModel model);

        Task<EmployeeModel> UpdateMeAsync(EmployeeModel model);

        Task<bool> UpdateImageAsync(int employeeId, string imageName);

        Task<bool> DeleteImageAsync(int employeeId);

        #endregion Mobile

        Task<EmployeeModel> AddAsync(EmployeeModel model, int clientId, int userId);
        Task<EmployeeModel> GetAsync(int id, int clientId, int userId);
        Task<IEnumerable<EmployeeListModel>> GetAllEmployeeAsync(int clientId, int userId);
        Task<EmployeePagedList> QueryAsync(int clientId, int userId, int page, int size, string searchText, int? placeId, int? departmentId, int? status);
        Task<IEnumerable<EmployeeModel>> GetNewRegisterEmployeesAsync(int clientId,int userId);        
        Task<EmployeeUpdateModel> UpdateAsync(EmployeeUpdateModel model, int clientId, int userId);
        Task<EmployeeUpdateModel> ApproveEmployeeAsync(EmployeeUpdateModel model, int clientId, int userId);
        //status updated method
        Task<bool> UpdateActiveStatusAsync(int employeeId, int clientId, int userId, int status);
        Task<bool> UpdateInActiveStatusAsync(int employeeId, int clientId, int userId, int status);
        Task<bool> UpdateRejectedStatusAsync(int employeeId, int clientId, int userId, int status);        
    }
}
