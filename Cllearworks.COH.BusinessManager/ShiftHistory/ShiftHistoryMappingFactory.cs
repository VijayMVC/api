using Cllearworks.COH.Models.ShiftHistory;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.ShiftHistory
{
    public class ShiftHistoryMappingFactory : IMappingFactory<ShiftEmployeeHistory, ShiftHistoryModel, ShiftHistoryModel>
    {
        public ShiftEmployeeHistory ConvertToDataModel(ShiftHistoryModel model)
        {
            if (model == null)
                return null;

            var dataModel = new ShiftEmployeeHistory();

            dataModel.Id = model.Id;
            dataModel.ShiftId = model.ShiftId;
            dataModel.StartDate = model.StartDate;
            dataModel.EndDate = model.EndDate;
            dataModel.EmployeeId = model.EmployeeId;

            return dataModel;
        }

        public ShiftHistoryModel ConvertToListModel(ShiftEmployeeHistory dataModel)
        {
            throw new NotImplementedException();
        }

        public ShiftHistoryModel ConvertToModel(ShiftEmployeeHistory dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new ShiftHistoryModel();

            model.Id = dataModel.Id;
            model.ShiftId = dataModel.ShiftId;
            model.EmployeeId = dataModel.EmployeeId;
            model.StartDate = dataModel.StartDate;
            model.EndDate = dataModel.EndDate;
            

            return model;
        }
    }
}
