using Cllearworks.COH.Repository.ShiftHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.ShiftHistories
{
    public class ShiftHistoryRepository : BaseRepository, IShiftHistoryRepository
    {
        public async Task<ShiftEmployeeHistory> AddAsync(ShiftEmployeeHistory dataModel)
        {
            Context.ShiftEmployeeHistories.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<ShiftEmployeeHistory> GetAsync(int id)
        {
            return await Context.ShiftEmployeeHistories.FindAsync(id);
        }

        public async Task<ShiftEmployeeHistory> GetByEmployeeAsync(int empId)
        {
            return await Task.Run(() =>
            {
                return Context.ShiftEmployeeHistories.Where(e => e.EmployeeId == empId && e.EndDate == null).FirstOrDefault();
            });
        }

        public async Task<ShiftEmployeeHistory> UpdateAsync(ShiftEmployeeHistory dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.EndDate = dataModel.EndDate;

            await Context.SaveChangesAsync();

            return await GetAsync(dataModel.Id);
        }

    }
}
