using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Employees;
using Cllearworks.COH.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public class ChangeRequestManager : IChangeRequestManager
    {
        private readonly IChangeRequestRepository _changeRequestRepository;
        private readonly IMappingFactory<ChangeRequest, DeviceChangeRequestModel, DeviceChangeRequestModel> _deviceChangeRequestMapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPermissionManager _permissionManager;
        
        public ChangeRequestManager(IChangeRequestRepository changeRequestRepository,
            IMappingFactory<ChangeRequest, DeviceChangeRequestModel, DeviceChangeRequestModel> deviceChangeRequestMapper,
            IPermissionManager permissionManager,
            IEmployeeRepository employeeRepository)
        {
            _changeRequestRepository = changeRequestRepository;
            _deviceChangeRequestMapper = deviceChangeRequestMapper;
            _permissionManager = permissionManager;
            _employeeRepository = employeeRepository;
        }

        #region Mobile

        public async Task<DeviceChangeRequestModel> AddAsync(DeviceChangeRequestModel model)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(model.DeviceId))
                throw new Exception("Device id is required");
            if (string.IsNullOrEmpty(model.Email))
                throw new Exception("Email id is required");

            var existingDataModel = _employeeRepository.GetEmployeeByEmail(model.Email);
            if (existingDataModel == null || existingDataModel.Status != (int)EmployeeStatus.Active)
                throw new COHHttpException(HttpStatusCode.NotFound, false, "You are not our registered employee");

            var exist = _changeRequestRepository.IsChangeRequestExist(model.Email, model.DeviceId);
            if (exist)
                throw new COHHttpException(HttpStatusCode.Found, false, "You have already created device change request with this email id");

            var changeRequest = _deviceChangeRequestMapper.ConvertToDataModel(model);
            changeRequest.Status = (int)ChangeRequestStatus.Pending;
            changeRequest.RequestedDate = DateTime.UtcNow;                        

            changeRequest = await _changeRequestRepository.AddAsync(changeRequest);

            return _deviceChangeRequestMapper.ConvertToModel(changeRequest);
        }
        

        #endregion Mobile

        public async Task<IEnumerable<DeviceChangeRequestModel>> QueryAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewEmployee))
                throw new Exception("User has not permission to perform this operation");

            var deviceChangeRequests = await _changeRequestRepository.QueryAsync(clientId);

            return deviceChangeRequests.ToList().Select(u => _deviceChangeRequestMapper.ConvertToModel(u));
        }

        public async Task<bool> DeleteDeviceChangeRequest(int requestId, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteEmployee))
                throw new Exception("User has not permission to perform this operation");

            return await _changeRequestRepository.DeleteAsync(requestId);
        }

        public async Task<bool> ApproveAsync(int requestId, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            var existDeviceRequest = await _changeRequestRepository.GetAsync(requestId);

            var existEmployee = _employeeRepository.GetEmployeeByEmail(existDeviceRequest.Email);

            if(existEmployee == null)
                throw new Exception("User has not extis or User has inactive.");

            return await _changeRequestRepository.ApproveAsync(requestId, userId);
        }
    }
}
