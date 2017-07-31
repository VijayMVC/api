using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Employees
{
    public interface IChangeRequestRepository
    {
        #region Auth

        bool IsChangeRequestExist(string email, string password);

        #endregion Auth

        #region Mobile
        Task<ChangeRequest> AddAsync(ChangeRequest dataModel);
        #endregion Mobile

        Task<ChangeRequest> GetAsync(int requestId);
        Task<bool> ApproveAsync(int requestId, int userId);
        Task<bool> DeleteAsync(int requestId);
        Task<IQueryable<ChangeRequest>> QueryAsync(int clientId);
    }
}
