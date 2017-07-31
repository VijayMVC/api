using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Places
{
    public class PlaceRepository : BaseRepository ,IPlaceRepository
    {
        public async Task<Place> AddAsync(Place dataModel)
        {
            Context.Places.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            data.IsActive = false;            
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Place> GetAsync(int id)
        {
            return await Context.Places.FindAsync(id);
        }

        public async Task<IQueryable<Place>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Places.Where(p => p.ClientId == clientId && p.IsActive == true);
            });
            
        }

        public async Task<Place> UpdateAsync(Place dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.Name = dataModel.Name;
            data.Address = dataModel.Address;

            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }
    }
}
