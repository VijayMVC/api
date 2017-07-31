using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Shifts
{
    public class ShiftRepository : BaseRepository, IShiftRepository
    {
        public async Task<Shift> AddAsync(Shift dataModel)
        {
            Context.Shifts.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);

        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            data.IsActive = false;
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Shift> GetAsync(int id)
        {
            return await Context.Shifts.FindAsync(id);
        }

        public async Task<IQueryable<Shift>> QueryAsync(int clientId)
        {
            return await Task.Run(() => {
                return Context.Shifts.Where(s => s.ClientId == clientId && s.IsActive == true);
            });
        }
    }
}
