using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public class EmployeeRegisterMappingFactory : IMappingFactory<Employee, EmployeeRegisterModel, EmployeeRegisterModel>
    {
        public Employee ConvertToDataModel(EmployeeRegisterModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Employee();

            dataModel.DeviceId = model.DeviceId;
            dataModel.GmcId = model.GmcId;
            dataModel.ApnId = model.ApnId;
            dataModel.FirstName = model.FirstName;
            dataModel.LastName = model.LastName;
            dataModel.Email = model.Email.ToLower();
            dataModel.PhoneNumber = model.Contact;

            return dataModel;
        }

        public EmployeeRegisterModel ConvertToListModel(Employee dataModel)
        {
            throw new NotImplementedException();
        }

        public EmployeeRegisterModel ConvertToModel(Employee dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new EmployeeRegisterModel();

            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.DeviceId = dataModel.DeviceId;
            model.GmcId = dataModel.GmcId;
            model.ApnId = dataModel.ApnId;
            model.Email = dataModel.Email;
            model.Contact = dataModel.PhoneNumber;

            return model;
        }
    }
}
