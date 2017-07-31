using Cllearworks.COH.Models.Applications;
using Cllearworks.COH.Repository;

namespace Cllearworks.COH.BusinessManager.Applications
{
    public class ApplicationMappingFactory
    {
        public Application ConvertToDataModel(ApplicationModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Application();
            dataModel.Id = model.Id;
            dataModel.Name = model.Name;
            dataModel.ClientId = model.ClientId;
            dataModel.ClientSecret = model.ClientSecret;
            dataModel.Scope = model.Scope;

            return dataModel;
        }

        public ApplicationModel ConvertToModel(Application dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new ApplicationModel();
            model.Id = dataModel.Id;
            model.Name = dataModel.Name;
            model.ClientId = dataModel.ClientId;
            model.ClientSecret = dataModel.ClientSecret;
            model.Scope = dataModel.Scope;

            return model;
        }
    }
}
