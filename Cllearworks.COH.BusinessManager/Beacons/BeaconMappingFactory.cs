using Cllearworks.COH.Models.Beacons;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Beacons
{
    public class BeaconMappingFactory : IMappingFactory<Beacon, BeaconModel, BeaconModel>
    {
        public Beacon ConvertToDataModel(BeaconModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Beacon();

            dataModel.Id = model.BeaconId;
            dataModel.MacAddress = model.MacAddress;
            dataModel.UUID = model.Uuid;
            dataModel.Major = model.Major;
            dataModel.Minor = model.Minor;                        
            dataModel.IsActive = model.IsActive;
            dataModel.BeaconType = (int) model.BeaconType;
            dataModel.PlaceId = model.PlaceId;
            dataModel.Name = model.Name;
            dataModel.DepartmentId = model.DepartmentId;
                        
            return dataModel;
        }

        public BeaconModel ConvertToListModel(Beacon dataModel)
        {
            throw new NotImplementedException();
        }

        public BeaconModel ConvertToModel(Beacon dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new BeaconModel();

            model.BeaconId = dataModel.Id;
            model.MacAddress = dataModel.MacAddress;
            model.Uuid = dataModel.UUID;
            model.Major = dataModel.Major;
            model.Minor = dataModel.Minor;
            model.IsActive = dataModel.IsActive;
            model.BeaconType = (BeaconTypes) dataModel.BeaconType;
            model.PlaceId = dataModel.PlaceId;
            model.Name = dataModel.Name;
            model.DepartmentId = dataModel.DepartmentId;
            model.PlaceName = dataModel.Place != null ? dataModel.Place.Name : "";
            model.DepartmentName = dataModel.Department != null ? dataModel.Department.Name : "";

            return model;
        }
    }
}
