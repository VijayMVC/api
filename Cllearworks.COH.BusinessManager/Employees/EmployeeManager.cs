using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.BusinessManager.ShiftHistory;
using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Models.ShiftHistory;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Applications;
using Cllearworks.COH.Repository.Employees;
using Cllearworks.COH.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Data;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public class EmployeeManager : IEmployeeManager
    {
        public IEmployeeRepository _employeeRepository;
        public IMappingFactory<Employee, EmployeeListModel, EmployeeModel> _employeeMapper;
        public IMappingFactory<Employee, EmployeeUpdateModel, EmployeeUpdateModel> _employeeUpdateMapper;
        public IMappingFactory<Employee, EmployeeRegisterModel, EmployeeRegisterModel> _employeeRegisterMapper;
        private readonly IPermissionManager _permissionManager;
        private readonly IShiftHistoryManager _shiftHistroyManager;
        private readonly IApplicationRepository _applicationRepository;

        public EmployeeManager(IEmployeeRepository employeeRepository, IMappingFactory<Employee, EmployeeListModel, EmployeeModel> employeeMapper, IPermissionManager permissionManager,
            IMappingFactory<Employee, EmployeeUpdateModel, EmployeeUpdateModel> employeeUpdateMapper,
            IMappingFactory<Employee, EmployeeRegisterModel, EmployeeRegisterModel> employeeRegisterMapper,
            IShiftHistoryManager shiftHistroyManager)
        {
            _employeeRepository = employeeRepository;
            _employeeMapper = employeeMapper;
            _permissionManager = permissionManager;
            _employeeUpdateMapper = employeeUpdateMapper;
            _employeeRegisterMapper = employeeRegisterMapper;
            _shiftHistroyManager = shiftHistroyManager;
            _applicationRepository = new ApplicationRepository();
        }

        #region Mobile

        public async Task<EmployeeModel> Login(string deviceId, string emailId)
        {
            var employee = await _employeeRepository.Login(deviceId, emailId);

            return _employeeMapper.ConvertToModel(employee);
        }

        public async Task<EmployeeModel> GetMeAsync(int id)
        {
            return _employeeMapper.ConvertToModel(await _employeeRepository.GetAsync(id));
        }

        public async Task<EmployeeRegisterModel> RegisterAsync(EmployeeRegisterModel model)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(model.DeviceId))
                throw new Exception("Device id is required");
            if (string.IsNullOrEmpty(model.Email))
                throw new Exception("Email id is required");
            if (string.IsNullOrEmpty(model.FirstName))
                throw new Exception("First name id is required");

            var existingDataModel = _employeeRepository.GetEmployeeByEmail(model.Email);
            if (existingDataModel != null)
                throw new COHHttpException(HttpStatusCode.Found, false, "You are already register with this email id");

            var app = await _applicationRepository.GetApplicationByClientId(model.ApplicationClientId);
            if (app == null)
                throw new Exception("Application is not registered");

            var client = app.Clients.FirstOrDefault();
            if (client == null || !client.IsActive)
                throw new Exception("Client is not registered or not active");

            var employee = _employeeRegisterMapper.ConvertToDataModel(model);
            employee.CreatedOn = DateTime.UtcNow;
            employee.UpdatedOn = DateTime.UtcNow;
            employee.ClientId = client.Id;
            employee.Status = (int)EmployeeStatus.Pending;

            employee = await _employeeRepository.RegisterAsync(employee);

            return _employeeRegisterMapper.ConvertToModel(employee);
        }

        public async Task<EmployeeModel> UpdateMeAsync(EmployeeModel model)
        {
            if (model == null)
                throw new ArgumentNullException();

            var existingDataModel = await _employeeRepository.GetAsync(model.Id);
            if (existingDataModel == null)
                throw new Exception("Employee does not exist which you trying to update");

            var employee = _employeeMapper.ConvertToDataModel(model);
            employee.UpdatedOn = DateTime.UtcNow;

            employee = await _employeeRepository.UpdateMeAsync(employee);

            return _employeeMapper.ConvertToModel(employee);
        }

        public async Task<bool> UpdateImageAsync(int employeeId, string imageName)
        {
            return await _employeeRepository.UpdateImageAsync(employeeId, imageName);
        }

        public async Task<bool> DeleteImageAsync(int employeeId)
        {
            return await _employeeRepository.UpdateImageAsync(employeeId, null);
        }

        #endregion Mobile

        public async Task<EmployeeModel> AddAsync(EmployeeModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var employee = _employeeMapper.ConvertToDataModel(model);
            employee.CreatedOn = DateTime.UtcNow;
            employee.UpdatedOn = DateTime.UtcNow;
            employee.ClientId = clientId;
            employee.Status = (int)EmployeeStatus.Pending;

            employee = await _employeeRepository.AddAsync(employee);

            return _employeeMapper.ConvertToModel(employee);
        }

        public async Task<EmployeeModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewEmployee))
                throw new Exception("User has not permission to perform this operation");

            return _employeeMapper.ConvertToModel(await _employeeRepository.GetAsync(id));
        }

        public async Task<EmployeePagedList> QueryAsync(int clientId, int userId, int page, int size, string searchText, int? placeId, int? departmentId, int? status)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewEmployee))
                throw new Exception("User has not permission to perform this operation");

            var employeesList = await _employeeRepository.QueryAsync(clientId);

            if (placeId != null)
            {
                employeesList = employeesList.Where(e => e.PlaceId == placeId.Value);
            }

            if (departmentId != null)
            {
                employeesList = employeesList.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (status != null)
            {
                employeesList = employeesList.Where(e => e.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                employeesList = employeesList.Where(e => e.Email.Contains(searchText) || e.FirstName.Contains(searchText) || e.LastName.Contains(searchText));
            }

            var employees = employeesList.OrderBy(o => o.FirstName)
                                    .Skip((page - 1) * size)
                                    .Take(size);

            var count = employeesList.Count();

            var pagedList = new EmployeePagedList();
            pagedList.Records = employees.ToList().Select(u => _employeeMapper.ConvertToListModel(u)).ToList();
            pagedList.TotalRecords = count;

            return pagedList;
        }

        public async Task<EmployeeUpdateModel> UpdateAsync(EmployeeUpdateModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            //check the employee shift extis or not
            var updateEmployeeShift = await _shiftHistroyManager.GetByEmployeeIdAsync(model.Id, clientId, userId);

            //This is used for update employee shift
            if (updateEmployeeShift == null || (updateEmployeeShift != null && updateEmployeeShift.ShiftId != model.ShiftId))
            {
                if (updateEmployeeShift != null)
                {
                    updateEmployeeShift.EndDate = DateTime.UtcNow;
                    await _shiftHistroyManager.UpdateAsync(updateEmployeeShift, clientId, userId);
                }
                //then afte update employee new shift timing
                var addNewEmployeeShift = new ShiftHistoryModel();
                addNewEmployeeShift.EmployeeId = model.Id;
                addNewEmployeeShift.ShiftId = model.ShiftId;
                addNewEmployeeShift.StartDate = DateTime.UtcNow;

                await _shiftHistroyManager.AddAsync(addNewEmployeeShift, clientId, userId);
            }

            var existingDataModel = await _employeeRepository.GetAsync(model.Id);
            if (existingDataModel == null)
                throw new Exception("Employee does not exist which you trying to update");

            var employee = _employeeUpdateMapper.ConvertToDataModel(model);
            employee.UpdatedOn = DateTime.UtcNow;

            employee = await _employeeRepository.UpdateAsync(employee);

            return _employeeUpdateMapper.ConvertToModel(employee);
        }

        public async Task<EmployeeUpdateModel> ApproveEmployeeAsync(EmployeeUpdateModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var existingemployee = await _employeeRepository.GetAsync(model.Id);
            if (existingemployee == null)
                throw new Exception("Employee does not exist which you trying to approve");

            //this is using for the set the shift time
            var employeeApprove = new ShiftHistoryModel();
            employeeApprove.EmployeeId = model.Id;
            employeeApprove.ShiftId = model.ShiftId;
            employeeApprove.StartDate = DateTime.UtcNow;

            await _shiftHistroyManager.AddAsync(employeeApprove, clientId, userId);

            //this is using for the approve the employee
            var employee = _employeeUpdateMapper.ConvertToDataModel(model);

            employee.UpdatedOn = DateTime.UtcNow;
            employee.Status = (int)EmployeeStatus.Active;
            employee.ApprovedBy = userId;

            employee = await _employeeRepository.ApproveEmployeeAsync(employee);

            return _employeeUpdateMapper.ConvertToModel(employee);

        }

        public async Task<IEnumerable<EmployeeModel>> GetNewRegisterEmployeesAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewEmployee))
                throw new Exception("User has not permission to perform this operation");

            var employees = await _employeeRepository.GetNewRegisterEmployeesAsync(clientId);

            return employees.ToList().Select(u => _employeeMapper.ConvertToModel(u));
        }

        public async Task<IEnumerable<EmployeeListModel>> GetAllEmployeeAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewEmployee))
                throw new Exception("User has not permission to perform this operation");

            var employeesList = (await _employeeRepository.QueryAsync(clientId)).Where(e => e.Status == (int)EmployeeStatus.Active);

            return employeesList.ToList().Select(u => _employeeMapper.ConvertToListModel(u)).ToList();
        }

        //update status inactive to active
        public async Task<bool> UpdateActiveStatusAsync(int employeeId, int clientId, int userId, int status)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            var empdata = await _employeeRepository.GetAsync(employeeId);

            if (empdata.Status == (int)EmployeeStatus.Rejected)
                return false;

            return await _employeeRepository.UpdateStatusAsync(employeeId, status);
        }

        //update status active to inactive
        public async Task<bool> UpdateInActiveStatusAsync(int employeeId, int clientId, int userId, int status)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            var empdata = await _employeeRepository.GetAsync(employeeId);

            if (empdata.Status == (int)EmployeeStatus.Rejected)
                return false;

            return await _employeeRepository.UpdateStatusAsync(employeeId, status);
        }

        //update status pending into rejected
        public async Task<bool> UpdateRejectedStatusAsync(int employeeId, int clientId, int userId, int status)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddEmployee))
                throw new Exception("User has not permission to perform this operation");

            var empdata = await _employeeRepository.GetAsync(employeeId);

            if (empdata.Status == (int)EmployeeStatus.Rejected)
                return false;

            return await _employeeRepository.UpdateStatusAsync(employeeId, status);
        }
        
    }
}
