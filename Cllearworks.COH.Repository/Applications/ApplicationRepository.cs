using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Applications
{
    public class ApplicationRepository : BaseRepository, IApplicationRepository
    {
        public async Task<Application> GetApplication(int id)
        {
            return await Task.Run(() =>
            {
                return Context.Applications.Where(a => a.Id == id).FirstOrDefault();
            });
        }

        public async Task<Application> GetApplicationByClientId(Guid clientId)
        {
            return await Task.Run(() =>
            {
                return Context.Applications.Where(a => a.ClientId == clientId).FirstOrDefault();
            });
        }

        public async Task<IQueryable<Application>> GetApplications()
        {
            return await Task.Run(() =>
            {
                return Context.Applications;
            });
        }

        #region Only for auth

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        public async Task<bool> VerifyApplicationSecretAsync(Guid clientId, Guid clientSecret)
        {
            return await Task.Run(() => {
                return Context.Applications.Any(a => a.ClientId == clientId && a.ClientSecret == clientSecret);
            });
        }

        #endregion Only for auth
    }
}
