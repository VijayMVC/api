using Cllearworks.COH.Models.Places;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Places
{
    public class PlaceMappingFactory : IMappingFactory<Place, PlaceModel, PlaceModel>
    {
        public Place ConvertToDataModel(PlaceModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Place();

            dataModel.Id = model.Id;
            dataModel.Name = model.Name;
            dataModel.Address = model.Address;
            dataModel.ClientId = model.ClientId;
            dataModel.IsActive = model.IsActive;

            return dataModel;
        }

        public PlaceModel ConvertToListModel(Place dataModel)
        {
            throw new NotImplementedException();
        }

        public PlaceModel ConvertToModel(Place dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new PlaceModel();

            model.Id = dataModel.Id;
            model.Name = dataModel.Name;
            model.Address = dataModel.Address;
            model.ClientId = dataModel.ClientId;
            model.IsActive = dataModel.IsActive;

            return model;
        }
    }
}
