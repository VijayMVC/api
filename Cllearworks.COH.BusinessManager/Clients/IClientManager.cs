using Cllearworks.COH.Models.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Clients
{
    public interface IClientManager
    {
        Task<ClientModel> AddAsync(ClientModel model, int userId);
        Task<ClientModel> GetAsync(int id, int userId);
        Task<ClientModel> GetByEmailAsync(string email);
        Task<IEnumerable<ClientModel>> QueryAsync(int userId);
        Task<ClientModel> UpdateAsync(ClientModel model, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
