using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Clients
{
    public class ClientRepository : BaseRepository, IClientRepository
    {
        public async Task<Client> AddAsync(Client dataModel)
        {
            Context.Clients.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<Client> GetAsync(int id)
        {
            return await Context.Clients.FindAsync(id);
        }

        public async Task<Client> GetByEmailAsync(string email)
        {
            return await Context.Clients.Where(x => x.Email == email).FirstOrDefaultAsync();
        }

        public async Task<IQueryable<Client>> QueryAsync()
        {
            return await Task.Run(() =>
            {
                return Context.Clients.Where(c => c.IsActive);
            });
        }

        public async Task<Client> UpdateAsync(Client dataModel)
        {
            var client = await GetAsync(dataModel.Id);

            client.FirstName = dataModel.FirstName;
            client.LastName = dataModel.LastName;
            client.Email = dataModel.Email;
            client.Address = dataModel.Address;
            client.SubscriptionPlan = dataModel.SubscriptionPlan;
            client.IsActive = dataModel.IsActive;
            //client.CreatedOn = dataModel.CreatedOn;
            client.UpdatedOn = dataModel.UpdatedOn;
            client.OrganizationName = dataModel.OrganizationName;

            await Context.SaveChangesAsync();

            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await Context.Clients.FindAsync(id);
            data.IsActive = false;
            return await Context.SaveChangesAsync() > 0;
        }
    }
}
