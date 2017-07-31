using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Holidays;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Holidays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Holidays
{
    public class HolidayManager : IHolidayManager
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly IMappingFactory<Holiday, HolidayModel, HolidayModel> _holidayMapper;
        private readonly IPermissionManager _permissionManager;

        public HolidayManager(IHolidayRepository holidayRepository, IMappingFactory<Holiday, HolidayModel, HolidayModel> holidayMapper, IPermissionManager permissionManager)
        {
            _holidayRepository = holidayRepository;
            _holidayMapper = holidayMapper;
            _permissionManager = permissionManager;
        }

        #region Mobile

        public async Task<IEnumerable<HolidayModel>> QueryMeAsync(int employeeId)
        {
            var holidays = await _holidayRepository.QueryByEmployeeAsync(employeeId);

            return holidays.ToList().Select(p => _holidayMapper.ConvertToModel(p));
        }

        #endregion Mobile

        public async Task<HolidayModel> AddAsync(HolidayModel model, int clientId, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddHoliday))
                throw new Exception("User has not permission to perform this operation");

            var result = await _holidayRepository.IsHolidayExistsAsync(clientId, model.Id, model.StartDate, model.EndDate);
            if (result)
                throw new Exception("Already inserted holiday");

            var holiday = _holidayMapper.ConvertToDataModel(model);

            holiday.ClientId = clientId;

            holiday = await _holidayRepository.AddAsync(holiday);
            return _holidayMapper.ConvertToModel(holiday);
        }

        public async Task<HolidayModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHoliday))
                throw new Exception("User has not permission to perform this operation");

            return _holidayMapper.ConvertToModel(await _holidayRepository.GetAsync(id));
        }

        public async Task<IEnumerable<HolidayModel>> QueryAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHoliday))
                throw new Exception("User has not permission to perform this operation");

            var holidays = (await _holidayRepository.QueryAsync(clientId)).OrderBy(h => h.StartDate);

            return holidays.ToList().Select(p => _holidayMapper.ConvertToModel(p));
        }

        public async Task<HolidayModel> UpdateAsync(HolidayModel model, int clientId, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditHoliday))
                throw new Exception("User has not permission to perform this operation");

            var result = await _holidayRepository.IsHolidayExistsAsync(clientId, model.Id, model.StartDate, model.EndDate);
            if (result)
                throw new Exception("Already inserted holiday");

            var holiday = _holidayMapper.ConvertToDataModel(model);

            holiday = await _holidayRepository.UpdateAsync(holiday);

            return _holidayMapper.ConvertToModel(holiday);
        }

        public async Task<bool> DeleteAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteHoliday))
                throw new Exception("User has not permission to perform this operation");

            return await _holidayRepository.DeleteAsync(id);
        }
    }
}
