using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Holidays
{
    public class HolidayRepository : BaseRepository, IHolidayRepository
    {
        #region Mobile

        public async Task<IQueryable<Holiday>> QueryByEmployeeAsync(int employeeId)
        {
            return await Task.Run(() =>
            {
                var query = (from e in Context.Employees
                             from h in Context.Holidays
                             join hd in Context.HolidayDetails on h.Id equals hd.HolidayId
                             where e.Id == employeeId
                             && (e.ClientId == hd.ToClient || e.PlaceId == hd.ToPlace || e.DepartmentId == hd.ToDepartment || e.Id == hd.ToEmployee)
                             select h).Distinct();

                return query;
            });
        }

        #endregion

        public async Task<Holiday> AddAsync(Holiday dataModel)
        {
            Context.Holidays.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            Context.HolidayDetails.RemoveRange(data.HolidayDetails);
            Context.Holidays.Remove(data);
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Holiday> GetAsync(int id)
        {
            return await Context.Holidays.FindAsync(id);
        }

        public async Task<IQueryable<Holiday>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Holidays.Where(p => p.ClientId == clientId);
            });

        }

        public async Task<Holiday> UpdateAsync(Holiday dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            Context.HolidayDetails.RemoveRange(data.HolidayDetails);
            await Context.SaveChangesAsync();

            data.Name = dataModel.Name;
            data.StartDate = dataModel.StartDate;
            data.EndDate = dataModel.EndDate;
            data.HolidayDetails = dataModel.HolidayDetails;

            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> IsHolidayExistsAsync(int clientId, int holidayId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => {
                var result = (from h in Context.Holidays
                              where h.ClientId == clientId && h.Id != holidayId &&
                              startDate <= h.EndDate && h.StartDate <= endDate
                              select h).Any();

                return result;
            });
        }
    }
}
