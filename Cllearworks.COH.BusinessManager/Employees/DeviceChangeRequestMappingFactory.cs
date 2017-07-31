using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public class DeviceChangeRequestMappingFactory : IMappingFactory<ChangeRequest, DeviceChangeRequestModel, DeviceChangeRequestModel>
    {
        public ChangeRequest ConvertToDataModel(DeviceChangeRequestModel model)
        {
            if (model == null)
                return null;

            var dataModel = new ChangeRequest();

            dataModel.Id = model.Id;
            dataModel.DeviceId = model.DeviceId;
            dataModel.GmcId = model.GmcId;
            dataModel.ApnId = model.ApnId;
            dataModel.Email = model.Email;

            return dataModel;
        }

        public DeviceChangeRequestModel ConvertToListModel(ChangeRequest dataModel)
        {
            throw new NotImplementedException();
        }

        public DeviceChangeRequestModel ConvertToModel(ChangeRequest dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new DeviceChangeRequestModel();

            model.Id = dataModel.Id;
            model.DeviceId = dataModel.DeviceId;
            model.GmcId = dataModel.GmcId;
            model.ApnId = dataModel.ApnId;
            model.Email = dataModel.Email;
            model.RequestDate = dataModel.RequestedDate;

            return model;
        }
    }
}
