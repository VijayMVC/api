using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Clients
{
    public interface IClientRepository
    {
        Task<Client> AddAsync(Client dataModel);
        Task<Client> GetAsync(int id);
        Task<Client> GetByEmailAsync(string email);

        Task<IQueryable<Client>> QueryAsync();
        Task<Client> UpdateAsync(Client dataModel);
        Task<bool> DeleteAsync(int id);
    }
}
