using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Dashboard;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository.Dashboard;
using Cllearworks.COH.Repository.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Dashboard
{
    public class DashboardManager : IDashboardManager
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IPermissionManager _permissionManager;
        private readonly IShiftRepository _shiftRepository;

        public DashboardManager(IDashboardRepository dashboardRepository, IPermissionManager permissionManager,
            IShiftRepository shiftRepository)
        {
            _dashboardRepository = dashboardRepository;
            _permissionManager = permissionManager;
            _shiftRepository = shiftRepository;
        }

        public async Task<GeneralStatsModel> GetGeneralStatsByClient(int clientId, int userId, int? placeId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewClientAdminDashboard))
                throw new Exception("User has not permission to perform this operation");

            return await _dashboardRepository.GetGeneralStatsByClient(clientId, placeId);
        }

        public async Task<IEnumerable<AttendanceTrendModel>> GetAttendanceTrendByClient(int clientId, int userId, int? placeId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewClientAdminDashboard))
                throw new Exception("User has not permission to perform this operation");

            var data = await _dashboardRepository.GetAttendanceTrendByClient(clientId, placeId);

            var incrementEmployees = 0;
            data.ToList().ForEach((a) =>
            {
                a.AttendanceTimeDt = a.AttendanceTimeDt;
                a.AttendanceTime = a.AttendanceTimeDt.ToString("HH:mm");
                incrementEmployees += a.Attendance;
                a.Attendance = incrementEmployees;
            });

            return data;
        }

        public async Task<IEnumerable<InTimeStatisticsModel>> GetInTimeStatisticsByClient(int clientId, int userId, int? placeId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewClientAdminDashboard))
                throw new Exception("User has not permission to perform this operation");

            var data = (await _dashboardRepository.GetInTimeStatisticsByClient(clientId, placeId)).ToList();

            var shifts = await _shiftRepository.QueryAsync(clientId);

            var shiftStartTime = DateTime.UtcNow.Date;

            if (shifts.Count() > 0)
            {
                var shiftDay = shifts.OrderBy(s => s.Id).FirstOrDefault().ShiftDetails.Where(sd => sd.DayOfWeek == (int)DateTime.Now.DayOfWeek).FirstOrDefault();
                if (shiftDay != null)
                {
                    if (shiftDay.IsWorkingDay)
                    {
                        shiftStartTime = shiftDay.StartTime.Value;
                    }
                }
            }

            var currentTime = DateTime.UtcNow;
            shiftStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, shiftStartTime.Hour, shiftStartTime.Minute, shiftStartTime.Second);

            var shiftStartTime2 = shiftStartTime.AddMinutes(30);
            var shiftStartTime3 = shiftStartTime2.AddMinutes(30);
            var shiftStartTime4 = shiftStartTime3.AddMinutes(60);

            var totalPresentEmployees = data.Count();

            var list = new List<InTimeStatisticsModel>();
            var model = new InTimeStatisticsModel();
            model.AttendanceTime = "Before " + shiftStartTime.ToString("HH:mm");
            model.AttendanceTimeDt = shiftStartTime;
            var dataCount = data.Where(a => a.AttendanceDate < shiftStartTime).Count();
            model.AttendancePercentage = (totalPresentEmployees != 0 && dataCount != 0) ? (dataCount * 100 / totalPresentEmployees) : 0;
            list.Add(model);

            model = new InTimeStatisticsModel();
            model.AttendanceTime = shiftStartTime.ToString("HH:mm") + " - " + shiftStartTime2.ToString("HH:mm");
            model.AttendanceTimeDt = shiftStartTime2;
            dataCount = data.Where(a => a.AttendanceDate >= shiftStartTime && a.AttendanceDate < shiftStartTime2).Count();
            model.AttendancePercentage = (totalPresentEmployees != 0 && dataCount != 0) ? (dataCount * 100 / totalPresentEmployees) : 0;
            list.Add(model);

            model = new InTimeStatisticsModel();
            model.AttendanceTime = shiftStartTime2.ToString("HH:mm") + " - " + shiftStartTime3.ToString("HH:mm");
            model.AttendanceTimeDt = shiftStartTime3;
            dataCount = data.Where(a => a.AttendanceDate >= shiftStartTime2 && a.AttendanceDate < shiftStartTime3).Count();
            model.AttendancePercentage = (totalPresentEmployees != 0 && dataCount != 0) ? (dataCount * 100 / totalPresentEmployees) : 0;
            list.Add(model);

            model = new InTimeStatisticsModel();
            model.AttendanceTime = shiftStartTime3.ToString("HH:mm") + " - " + shiftStartTime4.ToString("HH:mm");
            model.AttendanceTimeDt = shiftStartTime4;
            dataCount = data.Where(a => a.AttendanceDate >= shiftStartTime3 && a.AttendanceDate < shiftStartTime4).Count();
            model.AttendancePercentage = (totalPresentEmployees != 0 && dataCount != 0) ? (dataCount * 100 / totalPresentEmployees) : 0;
            list.Add(model);

            model = new InTimeStatisticsModel();
            model.AttendanceTime = "After " + shiftStartTime4.ToString("HH:mm");
            model.AttendanceTimeDt = shiftStartTime4;
            dataCount = data.Where(a => a.AttendanceDate >= shiftStartTime4).Count();
            model.AttendancePercentage = (totalPresentEmployees != 0 && dataCount != 0) ? (dataCount * 100 / totalPresentEmployees) : 0;
            list.Add(model);

            return list;
        }

        public async Task<IEnumerable<AttendanceTrendModel>> GetAttendanceStatisticsByClient(int clientId, int userId, int? placeId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewClientAdminDashboard))
                throw new Exception("User has not permission to perform this operation");

            var data = await _dashboardRepository.GetAttendanceStatisticsByClient(clientId, placeId);

            var totalPresentEmployees = data.Count();
            var todayDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);

            var list = new List<AttendanceTrendModel>();

            for (int i = 7; i > 0; i--)
            {
                var startDate = todayDate.AddDays(-i);
                var endDate = todayDate.AddDays(-(i-1));

                var dataCount = data.Where(a => a.AttendanceDate.Value >= startDate && a.AttendanceDate.Value < endDate).Count();

                var model = new AttendanceTrendModel();
                model.AttendanceTime = startDate.ToString("dd MMM");
                model.Attendance = dataCount;
                list.Add(model);
            }

            return list;
        }

        public async Task<IEnumerable<AttendanceTrendModel>> GetInOfficeEmployeesByClient(int clientId, int userId, int? placeId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewClientAdminDashboard))
                throw new Exception("User has not permission to perform this operation");

            var data = await _dashboardRepository.GetInOfficeEmployeesByClient(clientId, placeId);

            var totalPresentEmployees = data.Count();

            var shifts = await _shiftRepository.QueryAsync(clientId);

            var shiftTime = DateTime.UtcNow.Date;

            if (shifts.Count() > 0)
            {
                var shiftDay = shifts.OrderBy(s => s.Id).FirstOrDefault().ShiftDetails.Where(sd => sd.DayOfWeek == (int)DateTime.Now.DayOfWeek).FirstOrDefault();
                if (shiftDay != null && shiftDay.IsWorkingDay)
                {
                    shiftTime = shiftDay.StartTime.Value;
                }
            }

            var currentTime = DateTime.UtcNow;
            shiftTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, shiftTime.Hour, shiftTime.Minute, shiftTime.Second);

            var list = new List<AttendanceTrendModel>();

            var tracks = data.SelectMany(e => e.Tracks).Where(t => t.IsIn.Value).ToList();

            while (shiftTime < currentTime)
            {
                var dataCount = tracks.Where(t => t.FromTime.Value <= shiftTime && t.ToTime.Value > shiftTime).Count();

                var model = new AttendanceTrendModel();
                model.AttendanceTimeDt = shiftTime;
                model.AttendanceTime = shiftTime.ToString("HH:mm");
                model.Attendance = dataCount;
                list.Add(model);

                shiftTime = shiftTime.AddMinutes(1);
            }

            return list;
        }
    }
}
