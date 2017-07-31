using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Beacons
{
    public class BeaconRepository : BaseRepository, IBeaconRepository
    {
        public async Task<Beacon> AddAsync(Beacon dataModel)
        {
            Context.Beacons.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            data.IsActive = false;            
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Beacon> GetAsync(int id)
        {
            return await Context.Beacons.FindAsync(id);
        }

        public async Task<IQueryable<Beacon>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Beacons.Where(p => p.Place.ClientId == clientId && p.IsActive == true);
            });
        }

        public async Task<Beacon> UpdateAsync(Beacon dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.Name = dataModel.Name;
            data.MacAddress = dataModel.MacAddress;
            data.UUID = dataModel.UUID;
            data.Major = dataModel.Major;
            data.Minor = dataModel.Minor;
            data.BeaconType = dataModel.BeaconType;
            data.IsActive = dataModel.IsActive;
            data.PlaceId = dataModel.PlaceId;
            data.DepartmentId = dataModel.DepartmentId;
            data.UpdatedOn = dataModel.UpdatedOn;            
            await Context.SaveChangesAsync();

            return await GetAsync(dataModel.Id);
        }
    }
}
