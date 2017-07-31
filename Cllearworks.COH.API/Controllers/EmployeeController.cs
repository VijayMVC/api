using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Employees;
using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Utility;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Employee API
    /// </summary>
    [RoutePrefix("v1/clients/{clientId}/employees")]
    public class EmployeeController : BaseApiController
    {
        private IEmployeeManager _employeeManager;
        private IChangeRequestManager _changeRequestManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="employeeManager"></param>
        /// <param name="changeRequestManager"></param>
        public EmployeeController(IEmployeeManager employeeManager, IChangeRequestManager changeRequestManager)
        {
            _employeeManager = employeeManager;
            _changeRequestManager = changeRequestManager;
        }

        #region Mobile - Without Auth

        /// <summary>
        /// Mobile - Login
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="emailId"></param>
        /// <returns></returns>
        [Route("~/v1/Employees/Login")]
        [HttpGet]
        public async Task<IHttpActionResult> Login(string deviceId, string emailId)
        {
            var employee = await _employeeManager.Login(deviceId, emailId);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Get employee
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetEmployeeMe(int employeeId)
        {
            var employee = await _employeeManager.GetMeAsync(employeeId);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Update an employee
        /// </summary>
        /// <param name="model">Model of the employee</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateEmployee([FromBody] EmployeeModel model)
        {
            var employee = await _employeeManager.UpdateMeAsync(model);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Upload user image
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Image")]
        [HttpPost]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> UploadImage(int employeeId)
        {
            var httpRequest = HttpContext.Current.Request;

            foreach (string file in httpRequest.Files)
            {
                var postedFile = httpRequest.Files[file];
                if (postedFile != null && postedFile.ContentLength > 0)
                {

                    int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                    IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".jpeg", ".gif", ".png" };
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    var extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {
                        throw new Exception("Please upload image of type .jpg, .jpeg, .gif, .png.");
                    }
                    else if (postedFile.ContentLength > MaxContentLength)
                    {
                        throw new Exception("Please upload a file upto 1 mb.");
                    }
                    else
                    {
                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = HttpContext.Current.Server.MapPath("~/Images/" + fileName);
                        postedFile.SaveAs(filePath);

                        await _employeeManager.UpdateImageAsync(employeeId, fileName);

                        return Ok(Utilities.GetImageBasePath() + fileName);
                    }
                }

                throw new Exception("Please upload a image.");
            }
            throw new Exception("Please upload a image.");
        }

        /// <summary>
        /// Mobile - Remove user image
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Image")]
        [HttpDelete]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> RemoveImage(int employeeId)
        {
            var employee = await _employeeManager.GetMeAsync(employeeId);
            var filePath = HttpContext.Current.Server.MapPath("~/Images/" + employee.ImagePath);
            File.Delete(filePath);

            var result = await _employeeManager.DeleteImageAsync(employeeId);

            return Ok(result);
        }

        #endregion Mobile - Without Auth

        #region Mobile - Auth

        /// <summary>
        /// Mobile - Get employee
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me")]
        [HttpGet]
        public async Task<IHttpActionResult> GetEmployeeMe()
        {
            var employeeId = GetEmployeeId();
            var employee = await _employeeManager.GetMeAsync(employeeId);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Register new employee
        /// </summary>
        /// <param name="model">Register employee model</param>
        /// <returns></returns>
        [Route("~/v1/Employees/Register")]
        [HttpPost]
        public async Task<IHttpActionResult> RegisterEmployee([FromBody] EmployeeRegisterModel model)
        {
            var employee = await _employeeManager.RegisterAsync(model);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Update an employee
        /// </summary>
        /// <param name="model">Model of the employee</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateEmployeeMe([FromBody] EmployeeModel model)
        {
            var employee = await _employeeManager.UpdateMeAsync(model);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Add device change request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("~/v1/Employees/DeviceChangeRequest")]
        [HttpPost]
        public async Task<IHttpActionResult> DeviceChangeRequest([FromBody] DeviceChangeRequestModel model)
        {
            var employee = await _changeRequestManager.AddAsync(model);
            return Ok(employee);
        }

        /// <summary>
        /// Mobile - Upload user image
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Image")]
        [HttpPost]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> UploadImage()
        {
            var employeeId = GetEmployeeId();

            var httpRequest = HttpContext.Current.Request;

            foreach (string file in httpRequest.Files)
            {
                var postedFile = httpRequest.Files[file];
                if (postedFile != null && postedFile.ContentLength > 0)
                {

                    int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                    IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".jpeg", ".gif", ".png" };
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    var extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {
                        throw new Exception("Please upload image of type .jpg, .jpeg, .gif, .png.");
                    }
                    else if (postedFile.ContentLength > MaxContentLength)
                    {
                        throw new Exception("Please upload a file upto 1 mb.");
                    }
                    else
                    {
                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = HttpContext.Current.Server.MapPath("~/Images/" + fileName);
                        postedFile.SaveAs(filePath);

                        await _employeeManager.UpdateImageAsync(employeeId, fileName);

                        return Ok(Utilities.GetImageBasePath() + fileName);
                    }
                }

                throw new Exception("Please upload a image.");
            }
            throw new Exception("Please upload a image.");
        }

        /// <summary>
        /// Mobile - Remove user image
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Image")]
        [HttpDelete]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> RemoveImage()
        {
            var employeeId = GetEmployeeId();

            var employee = await _employeeManager.GetMeAsync(employeeId);
            var filePath = HttpContext.Current.Server.MapPath("~/Images/" + employee.ImagePath);
            File.Delete(filePath);

            var result = await _employeeManager.DeleteImageAsync(employeeId);

            return Ok(result);
        }

        #endregion Mobile - Auth

        #region CMS

        /// <summary>
        /// Get all employee List data
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("GetAllEmployee")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAllEmployees(int clientId)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.GetAllEmployeeAsync(clientId, userId);
            return Ok(employee);
        }

        /// <summary>
        /// Get all employeeList
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchText"></param>
        /// <param name="placeId"></param>
        /// <param name="departmentId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetEmployees(int clientId, int? page = 1, int? pageSize = 10, string searchText = null, int? placeId = null, int? departmentId = null, int? status = null)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.QueryAsync(clientId, userId, page.GetValueOrDefault(), pageSize.GetValueOrDefault(), searchText, placeId, departmentId, status);
            return Ok(employee);
        }

        /// <summary>
        /// Get all new register employee
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("register")]
        [HttpGet]
        public async Task<IHttpActionResult> GetNewRegisterEmployees(int clientId)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.GetNewRegisterEmployeesAsync(clientId, userId);
            return Ok(employee);
        }

        /// <summary>
        /// Get employee
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="employeeId">Id of the employee</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{employeeId}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetEmployee(int clientId, int employeeId)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.GetAsync(employeeId, clientId, userId);
            return Ok(employee);
        }

        /// <summary>
        /// Add an employee
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of the employee</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddEmployee(int clientId, [FromBody] EmployeeModel model)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.AddAsync(model, clientId, userId);
            return Ok(employee);
        }

        /// <summary>
        /// Update an employee
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of the employee</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateEmployee(int clientId, [FromBody] EmployeeUpdateModel model)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.UpdateAsync(model, clientId, userId);
            return Ok(employee);
        }

        /// <summary>
        /// Approve new register employee
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of the employee</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("approve")]
        [HttpPut]
        public async Task<IHttpActionResult> ApproveEmployee(int clientId, [FromBody] EmployeeUpdateModel model)
        {
            var userId = GetUserId();
            var employee = await _employeeManager.ApproveEmployeeAsync(model, clientId, userId);
            return Ok(employee);
        }

        /// <summary>
        /// Get All request for change device
        /// </summary>
        /// <param name="clientId">Id of client</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("requests")]
        [HttpGet]
        public async Task<IHttpActionResult> GetDeviceChangeRequest(int clientId)
        {
            var userId = GetUserId();
            var deviceChangeRequests = await _changeRequestManager.QueryAsync(clientId, userId);
            return Ok(deviceChangeRequests);
        }

        /// <summary>
        /// Reject device Change request
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("request/rejected/{requestId:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteDeviceChangeRequest(int clientId, int requestId)
        {
            var userId = GetUserId();
            return Ok(await _changeRequestManager.DeleteDeviceChangeRequest(requestId, clientId, userId));
        }

        /// <summary>
        /// Approve the new Device 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("request/approve/{requestId:int}")]
        [HttpPut]
        public async Task<IHttpActionResult> DeviceRequestApprove(int clientId, int requestId)
        {
            var userId = GetUserId();
            return Ok(await _changeRequestManager.ApproveAsync(requestId, clientId, userId));
        }

        /// <summary>
        /// Update Active Status
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("{employeeId:int}/Active")]
        [HttpPost]
        [UserAuthorize]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> UpdateActiveStatus(int clientId, int employeeId)
        {
            var userId = GetUserId();
            return Ok(await _employeeManager.UpdateActiveStatusAsync(employeeId, clientId, userId, (int)EmployeeStatus.Active));
        }

        /// <summary>
        /// Update InActive Status
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("{employeeId:int}/InActive")]
        [HttpPost]
        [UserAuthorize]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> UpdateInActiveStatus(int clientId, int employeeId)
        {
            var userId = GetUserId();
            return Ok(await _employeeManager.UpdateInActiveStatusAsync(employeeId, clientId, userId, (int)EmployeeStatus.InActive));
        }
        /// <summary>
        /// Update Rejected Status
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("{employeeId:int}/Rejected")]
        [HttpPost]
        [UserAuthorize]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> UpdateRejectedStatus(int clientId, int employeeId)
        {
            var userId = GetUserId();
            return Ok(await _employeeManager.UpdateRejectedStatusAsync(employeeId, clientId, userId, (int)EmployeeStatus.Rejected));
        }
        #endregion - CMS
    }
}
