using Cllearworks.COH.Models.Applications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Applications
{
    public interface IApplicationManager
    {
        Task<IEnumerable<ApplicationModel>> GetApplications();
        Task<ApplicationModel> GetApplication(int id);
        Task<ApplicationModel> GetApplicationByClientId(Guid clientId);
    }
}
