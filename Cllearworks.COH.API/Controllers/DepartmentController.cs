using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Departments;
using Cllearworks.COH.Models.Departments;
using Cllearworks.COH.Web.Utility.Auth;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Department API
    /// </summary>
    [UserAuthorize]
    [RoutePrefix("v1/clients/{clientId}/departments")]
    public class DepartmentController : BaseApiController
    {
        private IDepartmentManager _departmentManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="departmentManager"></param>
        public DepartmentController(IDepartmentManager departmentManager)
        {
            _departmentManager = departmentManager;
        }

        /// <summary>
        /// Get all department by client wise
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetDepartments(int clientId)
        {
            var userId = GetUserId();
            var departments = await _departmentManager.QueryAsync(clientId, userId);
            return Ok(departments);
        }

        /// <summary>
        /// Get department by departmentId 
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="departmentId">Id of the department</param>
        /// <returns></returns>
        [Route("{departmentId:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetDepartment(int clientId, int departmentId)
        {
            var userId = GetUserId();
            var department = await _departmentManager.GetAsync(departmentId, clientId, userId);
            return Ok(department);
        }

        /// <summary>
        /// Add department
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of department</param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddDepartment(int clientId, [FromBody] DepartmentModel model)
        {
            var userId = GetUserId();
            var department = await _departmentManager.AddAsync(model, clientId, userId);

            return Ok(department);
        }

        /// <summary>
        /// Update department
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of department</param>
        /// <returns></returns>
        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateDepartment(int clientId, [FromBody] DepartmentModel model)
        {
            var userId = GetUserId();
            var department = await _departmentManager.UpdateAsync(model, clientId, userId);
            return Ok(department);
        }

        /// <summary>
        /// Delete department
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="departmentId">Id of the department</param>
        /// <returns></returns>
        [Route("{departmentId:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteDepartment(int clientId, int departmentId)
        {
            var userId = GetUserId();
            return Ok(await _departmentManager.DeleteAsync(departmentId, clientId, userId));
        }
    }
}
