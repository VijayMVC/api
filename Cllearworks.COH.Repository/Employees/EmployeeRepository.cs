using Cllearworks.COH.Models.Employees;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Employees
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        #region Mobile

        public async Task<Employee> Login(string deviceId, string emailId)
        {
            return await Context.Employees.Where(x => x.DeviceId == deviceId && x.Email == emailId && x.Status == (int)EmployeeStatus.Active).FirstOrDefaultAsync();
        }

        public async Task<Employee> RegisterAsync(Employee dataModel)
        {
            Context.Employees.Add(dataModel);
            await Context.SaveChangesAsync();
            return await Context.Employees.FindAsync(dataModel.Id);
        }

        public async Task<Employee> UpdateMeAsync(Employee dataModel)
        {
            var data = await Context.Employees.FindAsync(dataModel.Id);

            data.FirstName = dataModel.FirstName;
            data.LastName = dataModel.LastName;
            //data.Email = dataModel.Email;
            data.PhoneNumber = dataModel.PhoneNumber;

            await Context.SaveChangesAsync();

            return await Context.Employees.FindAsync(dataModel.Id);
        }

        public async Task<bool> UpdateImageAsync(int employeeId, string imageName)
        {
            var data = await Context.Employees.FindAsync(employeeId);

            data.ImagePath = imageName;

            return await Context.SaveChangesAsync() > 0;
        }

        #endregion Mobile

        #region Auth

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Employee GetEmployeeByEmail(string email)
        {
            return Context.Employees.Where(u => u.Email == email).FirstOrDefault();
        }

        #endregion Auth

        public async Task<Employee> AddAsync(Employee dataModel)
        {
            Context.Employees.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<Employee> GetAsync(int id)
        {
            return await Context.Employees.FindAsync(id);
        }

        public async Task<IQueryable<Employee>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Employees.Where(e => e.ClientId == clientId && e.Status != (int)EmployeeStatus.Pending);
            });
        }

        public async Task<Employee> UpdateAsync(Employee dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.FirstName = dataModel.FirstName;
            data.LastName = dataModel.LastName;
            data.Email = dataModel.Email;   //put may if anyone wont change
            data.PhoneNumber = dataModel.PhoneNumber;            
            data.DepartmentId = dataModel.DepartmentId;
            data.PlaceId = dataModel.PlaceId;
            data.UpdatedOn = dataModel.UpdatedOn;

            await Context.SaveChangesAsync();

            return await GetAsync(dataModel.Id);
        }

        public async Task<Employee> ApproveEmployeeAsync(Employee dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.PlaceId = dataModel.PlaceId;
            data.DepartmentId = dataModel.DepartmentId;
            data.Status = dataModel.Status;
            data.ApprovedBy = dataModel.ApprovedBy;

            await Context.SaveChangesAsync();

            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> UpdateStatusAsync(int employeeId, int status)
        {
            var data = await GetAsync(employeeId);

            data.Status = status;

            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<IQueryable<Employee>> GetNewRegisterEmployeesAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Employees.Where(e => e.ClientId == clientId && e.Status == (int)EmployeeStatus.Pending);
            });
        }
    }
}
