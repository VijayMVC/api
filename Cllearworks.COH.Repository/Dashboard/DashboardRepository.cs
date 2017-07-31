using Cllearworks.COH.Models.Dashboard;
using Cllearworks.COH.Models.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Dashboard
{
    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        #region Client Dashboard

        public async Task<GeneralStatsModel> GetGeneralStatsByClient(int clientId, int? placeId)
        {
            return await Task.Run(() => 
            {
                var model = new GeneralStatsModel();

                model.TotalRegisteredEmployees = Context.Employees.Where(e => e.ClientId == clientId && (!placeId.HasValue || e.PlaceId == placeId) && e.Status == (int)EmployeeStatus.Active).Count();
                model.TotalNewRegisteredEmployees = Context.Employees.Where(e => e.ClientId == clientId && (!placeId.HasValue || e.PlaceId == placeId) && e.Status == (int)EmployeeStatus.Pending).Count();
                model.TotalDeviceChangeRequests = (from e in Context.Employees
                                                   join cr in Context.ChangeRequests on e.Email equals cr.Email
                                                   where e.ClientId == clientId &&
                                                   (!placeId.HasValue || e.PlaceId == placeId) &&
                                                   e.Status == (int)EmployeeStatus.Active &&
                                                   cr.Status == (int)ChangeRequestStatus.Pending
                                                   select cr).Count();

                try
                {
                    var spModel = Context.GetDailyAttendanceCount(clientId, (placeId.HasValue ? placeId : 0)).ToList();
                    if (spModel != null && spModel.Count() > 0)
                    {
                        model.TotalOntimeEmployees = spModel.ElementAt(0).Ontime.Value;
                        model.TotalLateEmployees = spModel.ElementAt(0).Late.Value;
                    }
                    else
                    {
                        model.TotalOntimeEmployees = 0;
                        model.TotalLateEmployees = 0;
                    }
                }
                catch (Exception)
                {
                    model.TotalOntimeEmployees = 0;
                    model.TotalLateEmployees = 0;
                }

                return model;
            });
        }

        public async Task<IEnumerable<AttendanceTrendModel>> GetAttendanceTrendByClient(int clientId, int? placeId)
        {
            return await Task.Run(() =>
            {
                var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                var maxDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);

                var list = (from a in Context.Attendances
                            join e in Context.Employees on a.EmployeeId equals e.Id
                            join c in Context.Clients on e.ClientId equals c.Id
                            where c.Id == clientId &&
                            (!placeId.HasValue || e.PlaceId == placeId) &&
                            e.Status == (int)EmployeeStatus.Active &&
                            a.AttendanceDate >= minDate && a.AttendanceDate <= maxDate
                            group a.Id by a.AttendanceDate into attGroup
                            select new AttendanceTrendModel() { AttendanceTimeDt = attGroup.Key.Value, AttendanceTime = attGroup.Key.ToString(), Attendance = attGroup.Count() }).ToList();

                return list;
            });
        }

        public async Task<IQueryable<Attendance>> GetInTimeStatisticsByClient(int clientId, int? placeId)
        {
            return await Task.Run(() =>
            {
                var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                var maxDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);

                var list = (from a in Context.Attendances
                            join e in Context.Employees on a.EmployeeId equals e.Id
                            join c in Context.Clients on e.ClientId equals c.Id
                            where c.Id == clientId &&
                            (!placeId.HasValue || e.PlaceId == placeId) &&
                            e.Status == (int)EmployeeStatus.Active &&
                            a.AttendanceDate >= minDate && a.AttendanceDate <= maxDate
                            select a);

                return list;
            });
        }

        public async Task<IQueryable<Attendance>> GetAttendanceStatisticsByClient(int clientId, int? placeId)
        {
            return await Task.Run(() =>
            {
                var list = (from a in Context.Attendances
                            join e in Context.Employees on a.EmployeeId equals e.Id
                            join c in Context.Clients on e.ClientId equals c.Id
                            where c.Id == clientId &&
                            (!placeId.HasValue || e.PlaceId == placeId) &&
                            e.Status == (int)EmployeeStatus.Active
                            select a);

                return list;
            });
        }

        public async Task<IQueryable<Attendance>> GetInOfficeEmployeesByClient(int clientId, int? placeId)
        {
            return await Task.Run(() =>
            {
                var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                var maxDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);

                var list = (from a in Context.Attendances
                            join e in Context.Employees on a.EmployeeId equals e.Id
                            join c in Context.Clients on e.ClientId equals c.Id
                            where c.Id == clientId &&
                            (!placeId.HasValue || e.PlaceId == placeId) &&
                            e.Status == (int)EmployeeStatus.Active &&
                            a.AttendanceDate >= minDate && a.AttendanceDate <= maxDate
                            select a);

                return list;
            });
        }

        #endregion Client Dashboard
    }
}
