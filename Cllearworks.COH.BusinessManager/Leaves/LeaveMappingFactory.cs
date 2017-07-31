using Cllearworks.COH.Models.Leaves;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Leaves
{
    public class LeaveMappingFactory : IMappingFactory<Leave, LeaveModel, LeaveModel>
    {
        public Leave ConvertToDataModel(LeaveModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Leave();

            dataModel.Id = model.Id;
            dataModel.LeaveType = (int)model.LeaveType;
            dataModel.StartDate = model.StartDate;
            dataModel.EndDate = model.EndDate;
            dataModel.Reason = model.Reason;
            dataModel.EmployeeId = model.EmployeeId;
            dataModel.Status = model.Status.HasValue ? (int)model.Status : (int)LeaveStatus.Pending;

            return dataModel;
        }

        public LeaveModel ConvertToListModel(Leave dataModel)
        {
            throw new NotImplementedException();
        }

        public LeaveModel ConvertToModel(Leave dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new LeaveModel();

            model.Id = dataModel.Id;
            model.LeaveType = (LeaveTypes)dataModel.LeaveType;
            model.StartDate = dataModel.StartDate;
            model.EndDate = dataModel.EndDate;
            model.Reason = dataModel.Reason;
            model.EmployeeId = dataModel.EmployeeId;
            model.Status = (LeaveStatus)dataModel.Status;            
            model.ApprovedByUser = (dataModel.ApprovedByUser.HasValue) ? dataModel.ApprovedByUser.Value : 0;
            model.ApprovedByEmployee = (dataModel.ApprovedByEmployee.HasValue) ? dataModel.ApprovedByEmployee.Value : 0;
            model.TotalLeaveDays = (dataModel.StartDate == dataModel.EndDate) ? 1 : (model.EndDate - model.StartDate).TotalDays;

            return model;
        }
    }
}
