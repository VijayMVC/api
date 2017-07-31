using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Leaves;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Leaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Leaves
{
    public class LeaveManager : ILeaveManager
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IMappingFactory<Leave, LeaveModel, LeaveModel> _leaveMapper;
        private readonly IPermissionManager _permissionManager;

        public LeaveManager(ILeaveRepository leaveRepository, IMappingFactory<Leave, LeaveModel, LeaveModel> leaveMapper, IPermissionManager permissionManager)
        {
            _leaveRepository = leaveRepository;
            _leaveMapper = leaveMapper;
            _permissionManager = permissionManager;
        }

        #region Mobile

        public async Task<LeaveModel> AddMeAsync(LeaveModel model, int employeeId)
        {
            if (model == null)
                throw new ArgumentNullException();

            var leave = _leaveMapper.ConvertToDataModel(model);

            leave.EmployeeId = employeeId;
            leave.Status = (int)LeaveStatus.Pending;

            leave = await _leaveRepository.AddAsync(leave);

            return _leaveMapper.ConvertToModel(leave);
        }

        public async Task<LeaveModel> GetMeAsync(int id)
        {
            return _leaveMapper.ConvertToModel(await _leaveRepository.GetAsync(id));
        }

        public async Task<IEnumerable<LeaveModel>> QueryMeAsync(int employeeId)
        {
            var leaves = await _leaveRepository.QueryByEmployeeAsync(employeeId);

            return leaves.ToList().Select(p => _leaveMapper.ConvertToModel(p));
        }

        public async Task<LeaveModel> UpdateMeAsync(LeaveModel model)
        {
            if (model == null)
                throw new ArgumentNullException();

            var leave = _leaveMapper.ConvertToDataModel(model);

            leave = await _leaveRepository.UpdateAsync(leave);

            return _leaveMapper.ConvertToModel(leave);
        }

        public async Task<bool> DeleteMeAsync(int id)
        {
            return await _leaveRepository.DeleteAsync(id);
        }

        public async Task<bool> CancelMeAsync(int id, int employeeId)
        {
            return await _leaveRepository.CancelAsync(id, employeeId, 0);
        }

        #endregion Mobile

        public async Task<LeaveModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewLeave))
                throw new Exception("User has not permission to perform this operation");

            return _leaveMapper.ConvertToModel(await _leaveRepository.GetAsync(id));
        }

        public async Task<IEnumerable<LeaveModel>> QueryByEmployeeAsync(int employeeId, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewLeave))
                throw new Exception("User has not permission to perform this operation");

            var leaves = await _leaveRepository.QueryByEmployeeAsync(employeeId);

            return leaves.ToList().Select(p => _leaveMapper.ConvertToModel(p));
        }

        public async Task<LeavePagedList> QueryAsync(int clientId, int userId, int page, int pageSize, int? placeId, int? departmentId, int? status)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewLeave))
                throw new Exception("User has not permission to perform this operation");

            var leavesList = await _leaveRepository.QueryAsync(clientId);

            if (placeId != null)
            {
                leavesList = leavesList.Where(l => l.Employee1.PlaceId == placeId.Value);
            }

            if (departmentId != null)
            {
                leavesList = leavesList.Where(l => l.Employee1.DepartmentId == departmentId.Value);
            }

            var pagedLeaveList = new LeavePagedList();
            pagedLeaveList.TotalPendingRecords = leavesList.Where(l => l.Status == (int)LeaveStatus.Pending).Count();
            pagedLeaveList.TotalApprovedRecords = leavesList.Where(l => l.Status == (int)LeaveStatus.Approved).Count();
            pagedLeaveList.TotalRejectedRecords = leavesList.Where(l => l.Status == (int)LeaveStatus.Rejected).Count();
            pagedLeaveList.TotalCancelledRecords = leavesList.Where(l => l.Status == (int)LeaveStatus.Cancelled).Count();

            var count = leavesList.Count();
            pagedLeaveList.TotalRecords = count;

            if (status != null)
            {
                leavesList = leavesList.Where(l => l.Status == status.Value);
            }

            var leaves = leavesList.OrderByDescending(o => o.StartDate)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize);

            pagedLeaveList.Records = leaves.ToList().Select(u => _leaveMapper.ConvertToModel(u)).ToList();
            

            return pagedLeaveList;
        }

        public async Task<bool> ApproveAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditLeave))
                throw new Exception("User has not permission to perform this operation");

            return await _leaveRepository.ApproveAsync(id, userId);
        }

        public async Task<bool> RejectAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditLeave))
                throw new Exception("User has not permission to perform this operation");

            return await _leaveRepository.RejectAsync(id, userId);
        }

        public async Task<bool> CancelAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditLeave))
                throw new Exception("User has not permission to perform this operation");

            return await _leaveRepository.CancelAsync(id, 0, userId);
        }
    }
}
