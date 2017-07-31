using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Employees
{
    public interface IEmployeeRepository
    {
        #region Mobile
        Task<Employee> Login(string deviceId, string emailId);

        Task<Employee> RegisterAsync(Employee dataModel);

        Task<Employee> UpdateMeAsync(Employee dataModel);

        Task<bool> UpdateImageAsync(int employeeId, string imageName);

        #endregion Mobile

        #region Auth

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Employee GetEmployeeByEmail(string email);

        #endregion Auth

        Task<Employee> AddAsync(Employee dataModel);
        Task<Employee> GetAsync(int id);
        Task<IQueryable<Employee>> QueryAsync(int clientId);
        Task<IQueryable<Employee>> GetNewRegisterEmployeesAsync(int clientId);
        Task<Employee> UpdateAsync(Employee dataModel);
        Task<Employee> ApproveEmployeeAsync(Employee dataModel);
        Task<bool> UpdateStatusAsync(int employeeId, int status);
    }
}
