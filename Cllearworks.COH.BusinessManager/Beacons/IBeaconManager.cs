using Cllearworks.COH.Models.Beacons;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Beacons
{
    public interface IBeaconManager
    {
        #region Mobile

        Task<IEnumerable<BeaconModel>> QueryAsyncForMobile(int clientId);

        #endregion Mobile

        Task<BeaconModel> AddAsync(BeaconModel model, int clientId, int userId);
        Task<BeaconModel> GetAsync(int id, int clientId, int userId);        
        Task<IEnumerable<BeaconModel>> QueryAsync(int clientId, int userId);
        Task<BeaconModel> UpdateAsync(BeaconModel model, int clientId, int userId);
        Task<bool> DeleteAsync(int id, int clientId, int userId);
    }
}
