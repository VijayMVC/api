using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Holidays
{
    public interface IHolidayRepository
    {
        #region Mobile

        Task<IQueryable<Holiday>> QueryByEmployeeAsync(int employeeId);

        #endregion Mobile

        Task<Holiday> AddAsync(Holiday dataModel);
        Task<Holiday> GetAsync(int id);
        Task<IQueryable<Holiday>> QueryAsync(int clientId);
        Task<Holiday> UpdateAsync(Holiday dataModel);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsHolidayExistsAsync(int clientId, int holidayId, DateTime startDate, DateTime endDate);
    }
}
