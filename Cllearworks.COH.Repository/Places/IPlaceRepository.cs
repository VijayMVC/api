using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Places
{
    public interface IPlaceRepository
    {
        Task<Place> AddAsync(Place dataModel);
        Task<Place> GetAsync(int id);        
        Task<IQueryable<Place>> QueryAsync(int clientId);
        Task<Place> UpdateAsync(Place dataModel);
        Task<bool> DeleteAsync(int id);
    }
}
