using Cllearworks.COH.Models.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Places
{
    public interface IPlaceManager
    {
        Task<PlaceModel> AddAsync(PlaceModel model, int clientId, int userId);
        Task<PlaceModel> GetAsync(int id, int clientId, int userId);
        Task<IEnumerable<PlaceModel>> QueryAsync(int clientId, int userId);
        Task<PlaceModel> UpdateAsync(PlaceModel model, int clientId, int userId);
        Task<bool> DeleteAsync(int id, int clientId, int userId);
    }
}
