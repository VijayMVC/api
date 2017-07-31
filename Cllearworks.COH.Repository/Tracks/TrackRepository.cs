using Cllearworks.COH.Models.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Tracks
{
    public class TrackRepository : BaseRepository, ITrackRepository
    {
        #region Mobile

        public async Task<Track> AddAsync(Track dataModel)
        {
            return await Task.Run(() =>
            {
                dataModel.TrackDuration = (dataModel.ToTime.Value - dataModel.FromTime.Value).Ticks;

                var attendance = Context.Attendances.Where(a => a.Id == dataModel.AttendanceId).FirstOrDefault();

                if (attendance == null)
                    return null;

                var defaultBeacon = attendance.Employee.Client.Places.FirstOrDefault().Beacons.FirstOrDefault().Id;

                dataModel.FromBeacon = dataModel.FromBeacon == 0 ? defaultBeacon : dataModel.FromBeacon;

                Context.Tracks.Add(dataModel);
                Context.SaveChanges();
                return dataModel;
            });
        }

        public async Task<IEnumerable<Track>> GetAllByDayAsync(int employeeId)
        {
            //var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
            //var maxDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);

            return await Task.Run(() =>
            {
                var attandance = Context.Attendances.Where(a => a.EmployeeId == employeeId && a.AttendanceDate.Value.Day == DateTime.UtcNow.Day && a.AttendanceDate.Value.Month == DateTime.UtcNow.Month && a.AttendanceDate.Value.Year == DateTime.UtcNow.Year).FirstOrDefault();
                return Context.Tracks.Where(x => x.AttendanceId == attandance.Id).ToList();
            });
        }

        public async Task<long> TodayInDuration(int employeeId)
        {
            return await Task.Run(() =>
            {
                long totalInTime = 0;
                var attandance = Context.Attendances.Where(x => x.EmployeeId == employeeId &&
                x.AttendanceDate.Value.Day == DateTime.UtcNow.Day &&
                x.AttendanceDate.Value.Month == DateTime.UtcNow.Month &&
                x.AttendanceDate.Value.Year == DateTime.UtcNow.Year
                ).FirstOrDefault();
                var tracks = Context.Tracks.Where(x => x.IsIn == true && x.Attendance.EmployeeId == employeeId && x.AttendanceId == attandance.Id).ToList();
                foreach (var item in tracks)
                {
                    totalInTime += item.TrackDuration.Value;
                }
                return totalInTime;
            });
        }

        public async Task<long> TodayOutDuration(int employeeId)
        {
            return await Task.Run(() =>
            {
                long totalOutTime = 0;
                var attandance = Context.Attendances.Where(x => x.EmployeeId == employeeId &&
                x.AttendanceDate.Value.Day == DateTime.UtcNow.Day &&
                x.AttendanceDate.Value.Month == DateTime.UtcNow.Month &&
                x.AttendanceDate.Value.Year == DateTime.UtcNow.Year
                ).FirstOrDefault();
                var tracks = Context.Tracks.Where(x => x.IsOut == true && x.Attendance.EmployeeId == employeeId && x.AttendanceId == attandance.Id).ToList();
                foreach (var item in tracks)
                {
                    totalOutTime += item.TrackDuration.Value;
                }
                return totalOutTime;
            });
        }

        #endregion Mobile

        public async Task<IQueryable<Track>> GetTracksByAttendanceIdAsync(int attendanceid)
        {
            return await Task.Run(() =>
            {
                var tracks = Context.Tracks.Where(e => e.AttendanceId == attendanceid);

                return tracks;
            });
        }
    }
}
