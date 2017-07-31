using Cllearworks.COH.Models.Shifts;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;

namespace Cllearworks.COH.BusinessManager.Shifts
{
    public class ShiftMappingFactory : IMappingFactory<Shift, ShiftModel, ShiftModel>
    {
        public Shift ConvertToDataModel(ShiftModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Shift();

            dataModel.Id = model.Id;
            dataModel.Name = model.Name;

            dataModel.ShiftDetails = new List<ShiftDetail>();

            if (model.ShiftDetails != null && model.ShiftDetails.Count > 0)
            {
                foreach (var day in model.ShiftDetails)
                {
                    var shiftDetail = new ShiftDetail();
                    shiftDetail.IsWorkingDay = day.IsWorkingDay;
                    shiftDetail.DayOfWeek = (int)day.DayOfWeek;
                    if (day.IsWorkingDay)
                    {
                        shiftDetail.StartTime = day.StartTime;
                        shiftDetail.EndTime = day.EndTime;
                        shiftDetail.WorkingHours = (decimal)day.EndTime.Value.Subtract(day.StartTime.Value).TotalHours - day.BreakHours;
                        shiftDetail.BreakHours = day.BreakHours;
                    }
                    dataModel.ShiftDetails.Add(shiftDetail);
                }
            }

            return dataModel;
        }

        public ShiftModel ConvertToListModel(Shift dataModel)
        {
            throw new NotImplementedException();
        }

        public ShiftModel ConvertToModel(Shift dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new ShiftModel();

            model.Id = dataModel.Id;
            model.Name = dataModel.Name;

            model.ShiftDetails = new List<WeekDayModel>();

            if (dataModel.ShiftDetails.Count > 0)
            {
                foreach (var day in dataModel.ShiftDetails)
                {
                    var shiftDetail = new WeekDayModel();
                    shiftDetail.IsWorkingDay = day.IsWorkingDay;
                    shiftDetail.DayOfWeek = (DayOfWeek)day.DayOfWeek;
                    shiftDetail.DayOfWeekName = ((DayOfWeek)day.DayOfWeek).ToString();
                    shiftDetail.StartTime = day.StartTime;
                    shiftDetail.EndTime = day.EndTime;
                    shiftDetail.BreakHours = day.BreakHours;
                    shiftDetail.WorkingHours = day.WorkingHours;
                    model.ShiftDetails.Add(shiftDetail);
                }
            }

            return model;
        }
    }
}
