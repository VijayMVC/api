using Cllearworks.COH.Models.Shifts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Shifts
{
    public interface IShiftManager
    {
        Task<ShiftModel> AddAsync(ShiftModel model, int clientId, int userId);
        Task<ShiftModel> GetAsync(int id, int clientId, int userId);
        Task<IEnumerable<ShiftModel>> QueryAsync(int clientId, int userId);
        Task<bool> DeleteAsync(int id, int clientId, int userId);
    }
}
