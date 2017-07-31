using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Shifts;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Shifts
{
    public class ShiftManager : IShiftManager
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IMappingFactory<Shift, ShiftModel, ShiftModel> _shiftMapper;
        private readonly IPermissionManager _permissionManager;

        public ShiftManager(IShiftRepository shiftRepository, IMappingFactory<Shift, ShiftModel, ShiftModel> shiftMapper, IPermissionManager permissionManager)
        {
            _shiftRepository = shiftRepository;
            _shiftMapper = shiftMapper;
            _permissionManager = permissionManager;
        }

        public async Task<ShiftModel> AddAsync(ShiftModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddShift))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var shift = _shiftMapper.ConvertToDataModel(model);

            shift.CreatedOn = DateTime.UtcNow;
            shift.UpdatedOn = DateTime.UtcNow;
            shift.CreatedBy = userId;
            shift.ClientId = clientId;
            shift.IsActive = true;

            shift = await _shiftRepository.AddAsync(shift);
            return _shiftMapper.ConvertToModel(shift);
        }

        public async Task<bool> DeleteAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteShift))
                throw new Exception("User has not permission to perform this operation");

            return await _shiftRepository.DeleteAsync(id);
        }

        public async Task<ShiftModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewShift))
                throw new Exception("User has not permission to perform this operation");

            return _shiftMapper.ConvertToModel(await _shiftRepository.GetAsync(id));
        }

        public async Task<IEnumerable<ShiftModel>> QueryAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewShift))
                throw new Exception("User has not permission to perform this operation");

            var shiftMaster = await _shiftRepository.QueryAsync(clientId);

            return shiftMaster.ToList().Select(u => _shiftMapper.ConvertToModel(u));
        }
    }
}
