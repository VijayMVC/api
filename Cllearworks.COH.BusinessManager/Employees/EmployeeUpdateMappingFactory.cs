using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Employees
{
    public class EmployeeUpdateMappingFactory : IMappingFactory<Employee, EmployeeUpdateModel, EmployeeUpdateModel>
    {
        public Employee ConvertToDataModel(EmployeeUpdateModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Employee();

            dataModel.Id = model.Id;
            dataModel.FirstName = model.FirstName;
            dataModel.LastName = model.LastName;
            dataModel.Email = model.Email;
            dataModel.PhoneNumber = model.Contact;
            dataModel.PlaceId = model.PlaceId;
            dataModel.DepartmentId = model.DepartmentId;

            return dataModel;
        }

        public EmployeeUpdateModel ConvertToListModel(Employee dataModel)
        {
            throw new NotImplementedException();
        }

        public EmployeeUpdateModel ConvertToModel(Employee dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new EmployeeUpdateModel();

            model.Id = dataModel.Id;
            model.FirstName = dataModel.FirstName;
            model.LastName = dataModel.LastName;
            model.Email = dataModel.Email;
            model.Contact = dataModel.PhoneNumber;
            model.PlaceId = model.PlaceId;
            model.DepartmentId = model.DepartmentId;

            return model;
        }
    }
}
