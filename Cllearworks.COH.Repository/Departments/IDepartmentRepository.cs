using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Departments
{
    public interface IDepartmentRepository
    {
        Task<Department> AddAsync(Department dataModel);
        Task<Department> GetAsync(int id);
        Task<IQueryable<Department>> QueryAsync(int clientId);
        Task<Department> UpdateAsync(Department dataModel);
        Task<bool> DeleteAsync(int id);
    }
}
