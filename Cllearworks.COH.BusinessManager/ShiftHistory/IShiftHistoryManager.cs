using Cllearworks.COH.Models.ShiftHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.ShiftHistory
{
    public interface IShiftHistoryManager
    {
        Task<ShiftHistoryModel> AddAsync(ShiftHistoryModel model, int clientId, int userId);        
        Task<ShiftHistoryModel> GetByEmployeeIdAsync(int empId, int clientId, int userId);
        Task<ShiftHistoryModel> UpdateAsync(ShiftHistoryModel model, int clientId, int userId);
    }
}
