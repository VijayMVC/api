using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Departments;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Departments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Departments
{
    public class DepartmentManager : IDepartmentManager
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMappingFactory<Department, DepartmentModel, DepartmentModel> _departmentMapper;
        private readonly IPermissionManager _permissionManager;

        public DepartmentManager(IDepartmentRepository departmentRepository, IMappingFactory<Department, DepartmentModel, DepartmentModel> departmentMapper, IPermissionManager permissionManager)
        {
            _departmentRepository = departmentRepository;
            _departmentMapper = departmentMapper;
            _permissionManager = permissionManager;
        }

        public async Task<DepartmentModel> AddAsync(DepartmentModel model, int clientId, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddDepartment))
                throw new Exception("User has not permission to perform this operation");

            var department = _departmentMapper.ConvertToDataModel(model);

            department.IsActive = true;
            department.CreatedOn = DateTime.UtcNow;
            department.UpdatedOn = DateTime.UtcNow;
            department.ClientId = clientId;

            department = await _departmentRepository.AddAsync(department);
            return _departmentMapper.ConvertToModel(department);
        }

        public async Task<bool> DeleteAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteDepartment))
                throw new Exception("User has not permission to perform this operation");

            return await _departmentRepository.DeleteAsync(id);
        }

        public async Task<DepartmentModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewDepartment))
                throw new Exception("User has not permission to perform this operation");

            return _departmentMapper.ConvertToModel(await _departmentRepository.GetAsync(id));
        }

        public async Task<IEnumerable<DepartmentModel>> QueryAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewDepartment))
                throw new Exception("User has not permission to perform this operation");

            var department = await _departmentRepository.QueryAsync(clientId);

            return department.ToList().Select(p => _departmentMapper.ConvertToModel(p));
        }

        public async Task<DepartmentModel> UpdateAsync(DepartmentModel model, int clientId, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditDepartment))
                throw new Exception("User has not permission to perform this operation");

            var department = _departmentMapper.ConvertToDataModel(model);

            department.UpdatedOn = DateTime.UtcNow;

            department = await _departmentRepository.UpdateAsync(department);

            return _departmentMapper.ConvertToModel(department);
        }
    }
}
