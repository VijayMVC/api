using Cllearworks.COH.Models.Departments;
using Cllearworks.COH.Repository;
using System;

namespace Cllearworks.COH.BusinessManager.Departments
{
    public class DepartmentMappingFactory : IMappingFactory<Department, DepartmentModel, DepartmentModel>
    {
        public Department ConvertToDataModel(DepartmentModel model)
        {
            if (model == null) return null;

            var dataModel = new Department();

            dataModel.Id = model.Id;
            dataModel.Name = model.Name;
            
            return dataModel;
        }

        public DepartmentModel ConvertToListModel(Department dataModel)
        {
            throw new NotImplementedException();
        }

        public DepartmentModel ConvertToModel(Department dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new DepartmentModel();

            model.Id = dataModel.Id;
            model.Name = dataModel.Name;

            return model;
        }
    }
}
