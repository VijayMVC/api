using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.ShiftHistory
{
    public interface IShiftHistoryRepository
    {
        Task<ShiftEmployeeHistory> AddAsync(ShiftEmployeeHistory dataModel);
        Task<ShiftEmployeeHistory> GetAsync(int id);
        Task<ShiftEmployeeHistory> GetByEmployeeAsync(int empId);
        Task<ShiftEmployeeHistory> UpdateAsync(ShiftEmployeeHistory dataModel);
    }
}
