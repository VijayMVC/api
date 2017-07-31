using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Shifts
{
    public interface IShiftRepository
    {
        Task<Shift> AddAsync(Shift dataModel);
        Task<Shift> GetAsync(int id);
        Task<IQueryable<Shift>> QueryAsync(int clientId);
        Task<bool> DeleteAsync(int id);
    }
}
