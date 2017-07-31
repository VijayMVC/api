using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Leaves
{
    public interface ILeaveRepository
    {
        Task<Leave> AddAsync(Leave dataModel);
        Task<Leave> GetAsync(int id);
        Task<IQueryable<Leave>> QueryByEmployeeAsync(int employeeId);
        Task<IQueryable<Leave>> QueryAsync(int clientId);
        Task<Leave> UpdateAsync(Leave dataModel);
        Task<bool> DeleteAsync(int id);

        Task<bool> ApproveAsync(int id, int userId);
        Task<bool> RejectAsync(int id, int userId);
        Task<bool> CancelAsync(int id, int employeeId, int userId);
    }
}
