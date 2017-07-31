using Cllearworks.COH.Models.Attendances;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Attendances
{
    public class AttendanceMappingFactory : IMappingFactory<Attendance, AttendanceModel, AttendanceModel>
    {
        public Attendance ConvertToDataModel(AttendanceModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Attendance();
            dataModel.Id = model.Id;
            dataModel.EmployeeId = model.EmployeeId;
            dataModel.AttendanceDate = model.AttendanceDate;
            dataModel.CheckInTime = model.CheckInTime;
            dataModel.CheckOutTime = model.CheckOutTime;
            //dataModel.TotalInTime = model.TotalInTime;
            //dataModel.TotalOutTime = model.TotalOutTime;
            dataModel.Remarks = model.Remarks;
            dataModel.IsPresent = model.IsPresent;

            return dataModel;
        }

        public AttendanceModel ConvertToListModel(Attendance dataModel)
        {
            throw new NotImplementedException();
        }

        public AttendanceModel ConvertToModel(Attendance dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new AttendanceModel();
            model.Id = dataModel.Id;
            model.EmployeeId = dataModel.EmployeeId;
            model.AttendanceDate = dataModel.AttendanceDate;
            model.CheckInTime = dataModel.CheckInTime;
            model.CheckOutTime = dataModel.CheckOutTime;
            model.TotalInTime = dataModel.TotalInTime.HasValue ? new TimeSpan(dataModel.TotalInTime.Value).TotalMinutes : 0;
            model.TotalOutTime = dataModel.TotalOutTime.HasValue ? new TimeSpan(dataModel.TotalOutTime.Value).TotalMinutes : 0;
            model.Remarks = dataModel.Remarks;
            model.IsPresent = dataModel.IsPresent;

            return model;
        }
    }
}
