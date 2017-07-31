using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Users
{
    public class UsersMappingFactory : IMappingFactory<User, UserModel, UserModel>
    {
        public User ConvertToDataModel(UserModel model)
        {
            if (model == null)
                return null;

            var dataModel = new User();
            dataModel.Id = model.Id;
            dataModel.FirstName = model.FirstName;
            dataModel.LastName = model.LastName;
            dataModel.Email = model.Email;
            dataModel.IsActive = model.IsActive;
            dataModel.Role = (int)model.Role;
            dataModel.ClientId = model.ClientId;
            
            return dataModel;
        }

        public UserModel ConvertToListModel(User dataModel)
        {
            throw new NotImplementedException();
        }

        public UserModel ConvertToModel(User dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new UserModel();
            model.Id = dataModel.Id;
            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.Email = dataModel.Email;
            model.IsActive = dataModel.IsActive;
            model.Role = (UserRoles)dataModel.Role;
            model.ClientId = dataModel.ClientId;

            return model;
        }
    }
}
