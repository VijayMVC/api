using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Beacons
{
    public interface IBeaconRepository
    {
       Task<Beacon> AddAsync(Beacon dataModel);
       Task<Beacon> GetAsync(int id);
       Task<IQueryable<Beacon>> QueryAsync(int clientId);
       Task<Beacon> UpdateAsync(Beacon dataModel);
       Task<bool> DeleteAsync(int id);
    }
}
