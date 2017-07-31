using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Applications
{
    public interface IApplicationRepository
    {
        Task<IQueryable<Application>> GetApplications();
        Task<Application> GetApplication(int id);
        Task<Application> GetApplicationByClientId(Guid clientId);

        #region Only for auth

        /// <summary>
        /// It is called in auth server, so dont change in this.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        Task<bool> VerifyApplicationSecretAsync(Guid clientId, Guid clientSecret);

        #endregion Only for auth
    }
}
