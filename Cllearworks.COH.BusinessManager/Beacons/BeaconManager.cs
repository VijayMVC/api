using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Beacons;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Beacons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Beacons
{
    public class BeaconManager : IBeaconManager
    {
        private readonly IBeaconRepository _beaconRepository;
        private readonly IMappingFactory<Beacon, BeaconModel, BeaconModel> _beaconMapper;
        private readonly IPermissionManager _permissionManager;

        public BeaconManager(IBeaconRepository beaconRepository, IMappingFactory<Beacon, BeaconModel, BeaconModel> beaconMapper, IPermissionManager permissionManager) {
            _beaconRepository = beaconRepository;
            _beaconMapper = beaconMapper;
            _permissionManager = permissionManager;
        }

        #region Mobile

        public async Task<IEnumerable<BeaconModel>> QueryAsyncForMobile(int clientId)
        {
            var beacons = await _beaconRepository.QueryAsync(clientId);

            return beacons.ToList().Select(u => _beaconMapper.ConvertToModel(u));
        }

        #endregion Mobile

        public async Task<BeaconModel> AddAsync(BeaconModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddBeacon))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var beacon = _beaconMapper.ConvertToDataModel(model);

            beacon.IsActive = true;
            beacon.CreatedOn = DateTime.UtcNow;
            beacon.UpdatedOn = DateTime.UtcNow;            

            beacon = await _beaconRepository.AddAsync(beacon);

            return _beaconMapper.ConvertToModel(beacon);
        }

        public async Task<BeaconModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewBeacon))
                throw new Exception("User has not permission to perform this operation");

            return _beaconMapper.ConvertToModel(await _beaconRepository.GetAsync(id));
        }

        public async Task<IEnumerable<BeaconModel>> QueryAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewBeacon))
                throw new Exception("User has not permission to perform this operation");

            var beacons = await _beaconRepository.QueryAsync(clientId);

            return beacons.ToList().Select(u => _beaconMapper.ConvertToModel(u));
        }

        public async Task<BeaconModel> UpdateAsync(BeaconModel model, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditBeacon))
                throw new Exception("User has not permission to perform this operation");

            if (model == null)
                throw new ArgumentNullException();

            var beacon = _beaconMapper.ConvertToDataModel(model);

            beacon.UpdatedOn = DateTime.UtcNow;

            beacon = await _beaconRepository.UpdateAsync(beacon);

            return _beaconMapper.ConvertToModel(beacon);
        }

        public async Task<bool> DeleteAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeleteBeacon))
                throw new Exception("User has not permission to perform this operation");

            return await _beaconRepository.DeleteAsync(id);
        }
    }
}
