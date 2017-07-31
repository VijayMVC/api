using Cllearworks.COH.Models.Clients;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Clients
{
    public class ClientMappingFactory : IMappingFactory<Client, ClientModel, ClientModel>
    {
        public Client ConvertToDataModel(ClientModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Client();
            dataModel.Id = model.Id;
            dataModel.FirstName = model.FirstName;
            dataModel.LastName = model.LastName;
            dataModel.Email = model.Email;
            dataModel.Address = model.Address;
            dataModel.SubscriptionPlan = (int)model.SubscriptionPlan;
            dataModel.IsActive = model.IsActive;
            dataModel.CreatedOn = model.CreatedOn;
            dataModel.UpdatedOn = model.UpdatedOn;
            dataModel.OrganizationName = model.OrganizationName;

            return dataModel;
        }

        public ClientModel ConvertToListModel(Client dataModel)
        {
            throw new NotImplementedException();
        }

        public ClientModel ConvertToModel(Client dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new ClientModel();
            model.Id = dataModel.Id;
            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.Email = dataModel.Email;
            model.Address = dataModel.Address;
            model.SubscriptionPlan = dataModel.SubscriptionPlan.HasValue ? (SubscriptionPlans)dataModel.SubscriptionPlan : SubscriptionPlans.SubscriptionPlan1;
            model.IsActive = dataModel.IsActive;
            model.CreatedOn = dataModel.CreatedOn;
            model.UpdatedOn = dataModel.UpdatedOn;
            model.OrganizationName = dataModel.OrganizationName;

            if (dataModel.Application != null)
            {
                model.ApplicationClientId = dataModel.Application.ClientId;
                model.ApplicationClientSecret = dataModel.Application.ClientSecret;
            }

            return model;
        }
    }
}
