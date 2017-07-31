using Cllearworks.COH.Models.Leaves;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Leaves
{
    public class LeaveRepository : BaseRepository, ILeaveRepository
    {
        public async Task<Leave> AddAsync(Leave dataModel)
        {
            Context.Leaves.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<Leave> GetAsync(int id)
        {
            return await Context.Leaves.FindAsync(id);
        }

        public async Task<IQueryable<Leave>> QueryByEmployeeAsync(int employeeId)
        {
            return await Task.Run(() =>
            {
                return Context.Leaves.Where(l => l.EmployeeId == employeeId);
            });
        }

        public async Task<IQueryable<Leave>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                var leaveData = (from c in Context.Clients
                                 join e in Context.Employees on c.Id equals e.ClientId
                                 join l in Context.Leaves on e.Id equals l.EmployeeId
                                 where c.Id == clientId
                                 select l);

                return leaveData;
                //return Context.Clients.Where(c => c.Id == clientId).SelectMany(e => e.Employees).SelectMany(e => e.Leaves);
            });
        }

        public async Task<Leave> UpdateAsync(Leave dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.LeaveType = dataModel.LeaveType;
            data.StartDate = dataModel.StartDate;
            data.EndDate = dataModel.EndDate;
            data.Reason = dataModel.Reason;
            data.EmployeeId = data.EmployeeId;
            data.Status = data.Status;

            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            Context.Leaves.Remove(data);
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ApproveAsync(int id, int userId)
        {
            var data = await GetAsync(id);
            data.ApprovedByUser = userId;
            data.Status = (int)LeaveStatus.Approved;
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectAsync(int id, int userId)
        {
            var data = await GetAsync(id);
            data.ApprovedByUser = userId;
            data.Status = (int)LeaveStatus.Rejected;
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelAsync(int id, int employeeId, int userId)
        {
            var data = await GetAsync(id);

            if (employeeId != 0)
            {
                data.ApprovedByEmployee = employeeId;
            }
            else
            {
                data.ApprovedByUser = userId;
            }

            data.Status = (int)LeaveStatus.Cancelled;
            return await Context.SaveChangesAsync() > 0;
        }
    }
}
