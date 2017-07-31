using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Places;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Places
{
    public class PlaceManager : IPlaceManager
    {
        private readonly IPlaceRepository _placeRepository;
        private readonly IMappingFactory<Place, PlaceModel, PlaceModel> _placeMapper;
        private readonly IPermissionManager _permissionManager;

        public PlaceManager(IPlaceRepository placeRepository, IMappingFactory<Place, PlaceModel, PlaceModel> placeMapper, IPermissionManager permissionManager) {
            _placeRepository = placeRepository;
            _placeMapper = placeMapper;
            _permissionManager = permissionManager;
        }

        public async Task<PlaceModel> AddAsync(PlaceModel model, int clientId, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanAddPlace))
                throw new Exception("User has not permission to perform this operation");

            var place = _placeMapper.ConvertToDataModel(model);

            place.ClientId = clientId;
            place.IsActive = true;
            place.CreatedOn = DateTime.UtcNow;
            place.UpdatedOn = DateTime.UtcNow;

            place = await _placeRepository.AddAsync(place);
            return _placeMapper.ConvertToModel(place);
        }

        public async Task<PlaceModel> GetAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewPlace))
                throw new Exception("User has not permission to perform this operation");

            return _placeMapper.ConvertToModel(await _placeRepository.GetAsync(id));
        }

        public async Task<IEnumerable<PlaceModel>> QueryAsync(int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewPlace))
                throw new Exception("User has not permission to perform this operation");

            var places = await _placeRepository.QueryAsync(clientId);

            return places.ToList().Select(p => _placeMapper.ConvertToModel(p));
        }

        public async Task<PlaceModel> UpdateAsync(PlaceModel model, int clientId, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanEditPlace))
                throw new Exception("User has not permission to perform this operation");

            var place = _placeMapper.ConvertToDataModel(model);
            
            place.UpdatedOn = DateTime.UtcNow;

            place = await _placeRepository.UpdateAsync(place);

            return _placeMapper.ConvertToModel(place);
        }

        public async Task<bool> DeleteAsync(int id, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanDeletePlace))
                throw new Exception("User has not permission to perform this operation");

            return await _placeRepository.DeleteAsync(id);
        }
    }
}
