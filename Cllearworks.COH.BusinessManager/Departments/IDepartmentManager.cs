using Cllearworks.COH.Models.Departments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Departments
{
    public interface IDepartmentManager
    {
        Task<DepartmentModel> AddAsync(DepartmentModel model, int clientId, int userId);
        Task<DepartmentModel> GetAsync(int id, int clientId, int userId);
        Task<IEnumerable<DepartmentModel>> QueryAsync(int clientId, int userId);
        Task<DepartmentModel> UpdateAsync(DepartmentModel model, int clientId, int userId);
        Task<bool> DeleteAsync(int id, int clientId, int userId);
    }
}
