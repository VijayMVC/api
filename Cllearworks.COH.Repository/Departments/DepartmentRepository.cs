using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Departments
{
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        public async Task<Department> AddAsync(Department dataModel)
        {
            Context.Departments.Add(dataModel);
            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await GetAsync(id);
            data.IsActive = false;
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Department> GetAsync(int id)
        {
            return await Context.Departments.FindAsync(id);
        }

        public async Task<IQueryable<Department>> QueryAsync(int clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Departments.Where(p => p.ClientId == clientId && p.IsActive == true);
            });
        }

        public async Task<Department> UpdateAsync(Department dataModel)
        {
            var data = await GetAsync(dataModel.Id);

            data.Name = dataModel.Name;

            await Context.SaveChangesAsync();
            return await GetAsync(dataModel.Id);
        }
    }
}
