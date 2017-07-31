using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cllearworks.COH.Models.ShiftHistory;
using Cllearworks.COH.Repository;
using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Repository.ShiftHistory;

namespace Cllearworks.COH.BusinessManager.ShiftHistory
{
    public class ShiftHistoryManager : IShiftHistoryManager
    {

        private readonly IShiftHistoryRepository _shiftHistroryRepository;
        private readonly IMappingFactory<ShiftEmployeeHistory, ShiftHistoryModel, ShiftHistoryModel> _shiftHistoryMapper;
        private readonly IPermissionManager _permissionManager;

        public ShiftHistoryManager(IShiftHistoryRepository shiftHistroryRepository, IMappingFactory<ShiftEmployeeHistory, ShiftHistoryModel, ShiftHistoryModel> shiftHistoryMapper, IPermissionManager permissionManager)
        {
            _shiftHistroryRepository = shiftHistroryRepository;
            _shiftHistoryMapper = shiftHistoryMapper;
            _permissionManager = permissionManager;
        }


        public async Task<ShiftHistoryModel> AddAsync(ShiftHistoryModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasUserClientAccessAsync(clientId, userId))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var shifthistory = _shiftHistoryMapper.ConvertToDataModel(model);

            shifthistory = await _shiftHistroryRepository.AddAsync(shifthistory);

            return _shiftHistoryMapper.ConvertToModel(shifthistory);
        }

        //public async Task<ShiftHistoryModel> GetAsync(int empId, int clientId, int userId)
        //{
        //    if (!await _permissionManager.HasUserClientAccessAsync(clientId, userId))
        //        throw new Exception("User has not permission to perform this operation");

        //    return _shiftHistoryMapper.ConvertToModel(await _shiftHistroryRepository.GetAsync(empId));
        //}

        public async Task<ShiftHistoryModel> UpdateAsync(ShiftHistoryModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasUserClientAccessAsync(clientId, userId))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var shifthistory = _shiftHistoryMapper.ConvertToDataModel(model);

            shifthistory = await _shiftHistroryRepository.UpdateAsync(shifthistory);

            return _shiftHistoryMapper.ConvertToModel(shifthistory);
        }

        public async Task<ShiftHistoryModel> GetByEmployeeIdAsync(int empId, int clientId, int userId)
        {
            if (!await _permissionManager.HasUserClientAccessAsync(clientId, userId))
                throw new Exception("User has not permission to perform this operation");
                        
            return _shiftHistoryMapper.ConvertToModel(await _shiftHistroryRepository.GetByEmployeeAsync(empId));
        }

    }
}
