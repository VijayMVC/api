using Cllearworks.COH.BusinessManager.Employees;
using Cllearworks.COH.BusinessManager.Tracks;
using Cllearworks.COH.Models.Attendances;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Attendances;
using Cllearworks.COH.Utility;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Users;

namespace Cllearworks.COH.BusinessManager.Attendances
{
    public class AttendanceManager : IAttendanceManager
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMappingFactory<Attendance, AttendanceModel, AttendanceModel> _attendanceMapper;
        private readonly IEmployeeManager _employeeManager;
        private readonly ITrackManager _trackManager;
        private readonly IPermissionManager _permissionManager;

        public AttendanceManager(IAttendanceRepository attendanceRepository, IMappingFactory<Attendance, AttendanceModel, AttendanceModel> attendanceMapper, IEmployeeManager employeeManager,
            ITrackManager trackManager, IPermissionManager permissionManager)
        {
            _attendanceRepository = attendanceRepository;
            _attendanceMapper = attendanceMapper;
            _employeeManager = employeeManager;
            _trackManager = trackManager;
            _permissionManager = permissionManager;
        }

        #region Mobile

        public async Task<AttendanceModel> CheckInAsync(int employeeId, string remarks = null)
        {
            if (employeeId == 0) throw new ArgumentException();
            var isExists = await _attendanceRepository.GetAttendanceForTodayAsync(employeeId, null);
            if (isExists != null) throw new COHHttpException(System.Net.HttpStatusCode.Found, false, "Already exists.");

            AttendanceModel model = new AttendanceModel();
            model.EmployeeId = employeeId;
            model.Remarks = remarks;
            model.CheckInTime = DateTime.UtcNow;
            model.AttendanceDate = DateTime.UtcNow;
            model.IsPresent = true;
            model.Remarks = string.Format("IN: {0}", remarks);

            var dataModel = await _attendanceRepository.CheckInAsync(_attendanceMapper.ConvertToDataModel(model));
            return _attendanceMapper.ConvertToModel(dataModel);
        }

        public async Task<AttendanceModel> CheckOutAsync(int employeeId, string remarks)
        {
            if (employeeId == 0) throw new ArgumentException();
            var model = await _attendanceRepository.GetAttendanceForTodayAsync(employeeId, null);

            if (model == null) throw new ArgumentNullException();

            model.CheckOutTime = DateTime.UtcNow;

            model.TotalInTime = await _trackManager.TodayInDuration(employeeId);

            model.TotalOutTime = await _trackManager.TodayOutDuration(employeeId);
            model.Remarks = string.Format("{0} OUT: {1}", model.Remarks, remarks);

            return _attendanceMapper.ConvertToModel(await _attendanceRepository.CheckOutAsync(model));
        }

        public async Task<AttendanceModel> GetAttendanceForTodayAsync(int employeeId, DateTime? attendanceDate = null)
        {
            return _attendanceMapper.ConvertToModel(await _attendanceRepository.GetAttendanceForTodayAsync(employeeId, attendanceDate));
        }

        public async Task<IEnumerable<AttendanceModel>> GetAttendanceByWeeklyAsync(int employeeId)
        {
            var employeeData = await _attendanceRepository.GetAttendanceByWeeklyAsync(employeeId);

            return employeeData;
        }

        public async Task<IEnumerable<AttendanceModel>> GetAttendanceByMonthlyAsync(int employeeId)
        {
            var employeeData = await _attendanceRepository.GetAttendanceByMonthlyAsync(employeeId);

            return employeeData.ToList().Select(u => _attendanceMapper.ConvertToModel(u)).ToList();
        }

        public async Task<AttendanceByYearModel> GetAttendanceByYearlyAsync(int employeeId)
        {
            var employeeData = await _attendanceRepository.GetAttendanceByYearlyAsync(employeeId);

            return employeeData;
        }

        #endregion Mobile
    }
}
