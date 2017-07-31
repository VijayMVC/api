using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using Cllearworks.COH.Models.Employees;

namespace Cllearworks.COH.Repository.Employees
{
    public class ChangeRequestRepository : BaseRepository, IChangeRequestRepository
    {
        #region Auth

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsChangeRequestExist(string email, string password)
        {
            return Context.ChangeRequests.Any(cr => cr.Email == email && cr.DeviceId == password && cr.Status == (int)ChangeRequestStatus.Pending);
        }

        #endregion Auth

        #region Mobile
        public async Task<ChangeRequest> AddAsync(ChangeRequest dataModel)
        {
            Context.ChangeRequests.Add(dataModel);
            await Context.SaveChangesAsync();
            return await Context.ChangeRequests.FindAsync(dataModel.Id);
        }
        #endregion Mobile

        public async Task<IQueryable<ChangeRequest>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                //return Context.ChangeRequests.Where(c => c.IsApproved == false && c.ApprovedBy == null);
                var data = (from e in Context.Employees
                            join cr in Context.ChangeRequests on e.Email equals cr.Email
                            where e.ClientId == clientId && e.Status == (int)EmployeeStatus.Active && cr.Status == (int)ChangeRequestStatus.Pending
                            select cr);
                return data;
            });
        }

        public async Task<ChangeRequest> GetAsync(int requestId)
        {
            return await Context.ChangeRequests.FindAsync(requestId);
        }

        public async Task<bool> ApproveAsync(int requestId, int userId)
        {
            var data = Context.ChangeRequests.Find(requestId);
            data.ApprovedBy = userId;
            data.Status = (int)ChangeRequestStatus.Approved;

            var employeeData = Context.Employees.Where(e => e.Email == data.Email && e.Status == (int)EmployeeStatus.Active).FirstOrDefault();

            employeeData.ApnId = data.ApnId;
            employeeData.GmcId = data.GmcId;
            employeeData.DeviceId = data.DeviceId;
            employeeData.UpdatedOn = DateTime.UtcNow;

            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            data.Status = (int)ChangeRequestStatus.Rejected;
            return await Context.SaveChangesAsync() > 0;
        }
    }
}
