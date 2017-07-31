using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Users
{
    public class UserMeMappingFactory : IMappingFactory<User, UserMeModel, UserMeModel>
    {
        public User ConvertToDataModel(UserMeModel model)
        {
            throw new NotImplementedException();
        }

        public UserMeModel ConvertToListModel(User dataModel)
        {
            throw new NotImplementedException();
        }

        public UserMeModel ConvertToModel(User dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new UserMeModel();
            model.Id = dataModel.Id;
            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.Email = dataModel.Email;
            model.IsActive = dataModel.IsActive;
            model.Role = (UserRoles)dataModel.Role;
            model.ClientId = dataModel.ClientId;
            model.Permissions = Permission.GetPermissions((UserRoles)dataModel.Role);

            return model;
        }
    }
}
