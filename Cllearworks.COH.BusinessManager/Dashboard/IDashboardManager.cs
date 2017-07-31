using Cllearworks.COH.Models.Dashboard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Dashboard
{
    public interface IDashboardManager
    {
        Task<GeneralStatsModel> GetGeneralStatsByClient(int clientId, int userId, int? placeId);
        Task<IEnumerable<AttendanceTrendModel>> GetAttendanceTrendByClient(int clientId, int userId, int? placeId);
        Task<IEnumerable<InTimeStatisticsModel>> GetInTimeStatisticsByClient(int clientId, int userId, int? placeId);
        Task<IEnumerable<AttendanceTrendModel>> GetAttendanceStatisticsByClient(int clientId, int userId, int? placeId);
        Task<IEnumerable<AttendanceTrendModel>> GetInOfficeEmployeesByClient(int clientId, int userId, int? placeId);
    }
}
