using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Models.Shifts;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Utility;
using System;
using System.Linq;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public class EmployeeMappingFactory : IMappingFactory<Employee, EmployeeListModel, EmployeeModel>
    {
        private readonly IMappingFactory<Shift, ShiftModel, ShiftModel> _shiftMapper;

        public EmployeeMappingFactory(IMappingFactory<Shift, ShiftModel, ShiftModel> shiftMapper)
        {
            _shiftMapper = shiftMapper;
        }

        public Employee ConvertToDataModel(EmployeeModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Employee();

            dataModel.Id = model.Id;
            dataModel.DeviceId = model.DeviceId;
            dataModel.GmcId = model.GmcId;
            dataModel.ApnId = model.ApnId;
            dataModel.FirstName = model.FirstName;
            dataModel.LastName = model.LastName;
            dataModel.Email = model.Email;
            dataModel.PhoneNumber = model.Contact;
            dataModel.Status = (int)model.Status;
            dataModel.ClientId = model.ClientId;
            dataModel.PlaceId = model.PlaceId;
            dataModel.EmployeeCode = model.EmployeeCode;
            dataModel.DepartmentId = model.DepartmentId;
            //dataModel.ImagePath = model.ImagePath;

            return dataModel;
        }

        public EmployeeListModel ConvertToListModel(Employee dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new EmployeeListModel();

            model.Id = dataModel.Id;
            model.EmployeeCode = dataModel.EmployeeCode;
            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.Email = dataModel.Email;
            model.Contact = dataModel.PhoneNumber;
            //model.WorkingHours = dataModel.WorkingHours.HasValue ? dataModel.WorkingHours.Value : 0;
            //model.BreakHours = dataModel.BreakHours.HasValue ? dataModel.BreakHours.Value : 0;
            model.Status = (EmployeeStatus) dataModel.Status;

            model.PlaceId = dataModel.PlaceId.HasValue ? dataModel.PlaceId : null;
            model.PlaceName = dataModel.Place != null ? dataModel.Place.Name : "";

            model.DepartmentId = dataModel.DepartmentId.HasValue ? dataModel.DepartmentId : null;
            model.DepartmentName = dataModel.Department != null ? dataModel.Department.Name : "";

            model.ImagePath = !string.IsNullOrEmpty(dataModel.ImagePath) ? Utilities.GetImageBasePath() + dataModel.ImagePath : "";

            if (dataModel.ShiftEmployeeHistories != null && dataModel.ShiftEmployeeHistories.Count != 0)
            {
                var shiftHistory = dataModel.ShiftEmployeeHistories.Where(s => s.EndDate == null).FirstOrDefault();
                if (shiftHistory != null)
                {
                    model.Shift = _shiftMapper.ConvertToModel(shiftHistory.Shift);
                    //var workingHours = model.Shift.ShiftDetails
                }

            }

            return model;
        }

        public EmployeeModel ConvertToModel(Employee dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new EmployeeModel();

            model.Id = dataModel.Id;
            model.DeviceId = dataModel.DeviceId;
            model.GmcId = dataModel.GmcId;
            model.ApnId = dataModel.ApnId;
            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.Email = dataModel.Email;
            model.Contact = dataModel.PhoneNumber;            
            //model.TotalWorkingHours = dataModel.WorkingHours != null ? dataModel.WorkingHours.ToString() : "";
            //model.TotalBreakHours = dataModel.BreakHours != null ? dataModel.BreakHours.ToString() : "";
            model.Status = (EmployeeStatus)dataModel.Status;

            model.ClientId = dataModel.ClientId;
            model.ClientName = dataModel.Client != null ? dataModel.Client.OrganizationName : "";

            model.PlaceId = dataModel.PlaceId.HasValue ? dataModel.PlaceId : null;
            model.PlaceName = dataModel.Place != null ? dataModel.Place.Name : "";

            model.EmployeeCode = dataModel.EmployeeCode;

            model.DepartmentId = dataModel.DepartmentId.HasValue ? dataModel.DepartmentId : null;
            model.DepartmentName = dataModel.Department != null ? dataModel.Department.Name : "";

            model.ImagePath = !string.IsNullOrEmpty(dataModel.ImagePath) ? Utilities.GetImageBasePath() + dataModel.ImagePath : "";
            model.CreatedOn = dataModel.CreatedOn;

            if (dataModel.ShiftEmployeeHistories != null && dataModel.ShiftEmployeeHistories.Count != 0)
            {
                var shiftHistory = dataModel.ShiftEmployeeHistories.Where(s => s.EndDate == null).FirstOrDefault();
                if (shiftHistory != null)
                {
                    model.ShiftId = shiftHistory.ShiftId;

                    model.Shift = _shiftMapper.ConvertToModel(shiftHistory.Shift);
                    var shiftDay = model.Shift.ShiftDetails.Where(sd => sd.DayOfWeek == DateTime.Now.DayOfWeek).FirstOrDefault();
                    if (shiftDay != null && shiftDay.IsWorkingDay)
                    {
                        model.InTime = shiftDay.StartTime.Value;
                        model.OutTime = shiftDay.EndTime.Value;
                        model.TotalWorkingHours = shiftDay.WorkingHours.ToString();
                        model.TotalBreakHours = shiftDay.BreakHours.ToString();
                    }
                }
                
            }

            return model;
        }
    }
}
