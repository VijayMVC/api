using Cllearworks.COH.Models.Holidays;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cllearworks.COH.BusinessManager.Holidays
{
    public class HolidayMappingFactory : IMappingFactory<Holiday, HolidayModel, HolidayModel>
    {
        public Holiday ConvertToDataModel(HolidayModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Holiday();

            dataModel.Id = model.Id;
            dataModel.Name = model.Name;
            dataModel.StartDate = model.StartDate;
            dataModel.EndDate = model.EndDate;

            dataModel.HolidayDetails = new List<HolidayDetail>();

            if (model.ToClient != null)
            {
                dataModel.HolidayDetails.Add(new HolidayDetail() { ToClient = model.ToClient });
            }

            if (model.ToPlace != null)
            {
                foreach (var place in model.ToPlace)
                {
                    var data = new HolidayDetail();
                    data.ToPlace = place;
                    dataModel.HolidayDetails.Add(data);
                }
            }

            if (model.ToDepartment != null)
            {
                foreach (var department in model.ToDepartment)
                {
                    var data = new HolidayDetail();
                    data.ToDepartment = department;
                    dataModel.HolidayDetails.Add(data);
                }
            }

            if (model.ToEmployee != null)
            {
                foreach (var employee in model.ToEmployee)
                {
                    var data = new HolidayDetail();
                    data.ToEmployee = employee;
                    dataModel.HolidayDetails.Add(data);
                }
            }

            return dataModel;
        }

        public HolidayModel ConvertToListModel(Holiday dataModel)
        {
            throw new NotImplementedException();
        }

        public HolidayModel ConvertToModel(Holiday dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new HolidayModel();

            model.Id = dataModel.Id;
            model.Name = dataModel.Name;
            model.StartDate = dataModel.StartDate;
            model.EndDate = dataModel.EndDate;

            if (dataModel.HolidayDetails.Count > 0)
            {
                if (dataModel.HolidayDetails.Any(h => h.ToClient.HasValue))
                {
                    model.ToClient = dataModel.HolidayDetails.Where(h => h.ToClient.HasValue).Select(h => h.ToClient).FirstOrDefault();
                }
                else if (dataModel.HolidayDetails.Any(h => h.ToPlace.HasValue))
                {
                    model.ToPlace = dataModel.HolidayDetails.Where(h => h.ToPlace.HasValue).Select(h => h.ToPlace).ToArray();
                }
                else if (dataModel.HolidayDetails.Any(h => h.ToDepartment.HasValue))
                {
                    model.ToDepartment = dataModel.HolidayDetails.Where(h => h.ToDepartment.HasValue).Select(h => h.ToDepartment).ToArray();
                }
                else if (dataModel.HolidayDetails.Any(h => h.ToEmployee.HasValue))
                {
                    model.ToEmployee = dataModel.HolidayDetails.Where(h => h.ToEmployee.HasValue).Select(h => h.ToEmployee).ToArray();
                }
            }

            return model;
        }
    }
}
