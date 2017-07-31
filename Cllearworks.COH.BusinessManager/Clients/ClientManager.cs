using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Clients;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Clients;
using Cllearworks.COH.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Clients
{
    public class ClientManager : IClientManager
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMappingFactory<Client, ClientModel, ClientModel> _clientMapper;
        private readonly IPermissionManager _permissionManager;

        public ClientManager(IClientRepository clientRepository, IMappingFactory<Client, ClientModel, ClientModel> clientMapper, IPermissionManager permissionManager)
        {
            _clientRepository = clientRepository;
            _clientMapper = clientMapper;
            _permissionManager = permissionManager;
        }

        public async Task<ClientModel> AddAsync(ClientModel model, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            var client = _clientMapper.ConvertToDataModel(model);
            client.IsActive = true;
            client.CreatedOn = DateTime.UtcNow;
            client.UpdatedOn = DateTime.UtcNow;

            client.Application = new Application()
            {
                Name = client.OrganizationName + "Mobile App",
                ClientId = Guid.NewGuid(),
                ClientSecret = Guid.NewGuid(),
                Scope = "mobile",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            var clientAdmin = new User()
            {
                FirstName = "Client",
                LastName = "Admin",
                Email = client.Email,
                Role = (int)UserRoles.ClientAdmin,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            var password = Guid.NewGuid().ToString().Substring(1, 6);

            string salt;
            string passwordHash;
            PasswordHelpers.GenerateSaltAndHash(password, out salt, out passwordHash);

            clientAdmin.Salt = salt;
            clientAdmin.PasswordHash = passwordHash;

            client.Users.Add(clientAdmin);

            client = await _clientRepository.AddAsync(client);

            return _clientMapper.ConvertToModel(client);
        }

        public async Task<ClientModel> GetAsync(int id, int userId)
        {
            if (!await _permissionManager.HasUserClientAccessAsync(id, userId))
                throw new Exception("User has not permission to perform this operation");

            return _clientMapper.ConvertToModel(await _clientRepository.GetAsync(id));
        }

        public async Task<ClientModel> GetByEmailAsync(string email)
        {
            return _clientMapper.ConvertToModel(await _clientRepository.GetByEmailAsync(email));
        }

        public async Task<IEnumerable<ClientModel>> QueryAsync(int userId)
        {
            if (!await _permissionManager.IsSuperAdmin(userId))
                throw new Exception("User has not permission to perform this operation");

            var clients = await _clientRepository.QueryAsync();

            return clients.ToList().Select(u => _clientMapper.ConvertToModel(u));
        }

        public async Task<ClientModel> UpdateAsync(ClientModel model, int userId)
        {
            if (model == null)
                throw new ArgumentNullException();

            if (!await _permissionManager.IsSuperAdmin(userId))
                throw new Exception("User has not permission to perform this operation");

            var existingDataModel = await _clientRepository.GetAsync(model.Id);
            if (existingDataModel == null)
                throw new Exception("Client does not exist which you trying to update");

            var employee = _clientMapper.ConvertToDataModel(model);
            employee.UpdatedOn = DateTime.UtcNow;

            employee = await _clientRepository.UpdateAsync(employee);

            return _clientMapper.ConvertToModel(employee);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            if (!await _permissionManager.IsSuperAdmin(userId))
                throw new Exception("User has not permission to perform this operation");

            return await _clientRepository.DeleteAsync(id);
        }
    }
}
