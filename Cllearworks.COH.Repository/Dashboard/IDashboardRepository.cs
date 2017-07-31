using Cllearworks.COH.Models.Dashboard;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Dashboard
{
    public interface IDashboardRepository
    {
        Task<GeneralStatsModel> GetGeneralStatsByClient(int clientId, int? placeId);
        Task<IEnumerable<AttendanceTrendModel>> GetAttendanceTrendByClient(int clientId, int? placeId);
        Task<IQueryable<Attendance>> GetInTimeStatisticsByClient(int clientId, int? placeId);
        Task<IQueryable<Attendance>> GetAttendanceStatisticsByClient(int clientId, int? placeId);
        Task<IQueryable<Attendance>> GetInOfficeEmployeesByClient(int clientId, int? placeId);
    }
}
