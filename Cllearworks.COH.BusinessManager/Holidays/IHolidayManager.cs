using Cllearworks.COH.Models.Holidays;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Holidays
{
    public interface IHolidayManager
    {
        #region Mobile

        Task<IEnumerable<HolidayModel>> QueryMeAsync(int employeeId);

        #endregion

        Task<HolidayModel> AddAsync(HolidayModel model, int clientId, int userId);
        Task<HolidayModel> GetAsync(int id, int clientId, int userId);
        Task<IEnumerable<HolidayModel>> QueryAsync(int clientId, int userId);
        Task<HolidayModel> UpdateAsync(HolidayModel model, int clientId, int userId);
        Task<bool> DeleteAsync(int id, int clientId, int userId);
    }
}
