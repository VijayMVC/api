using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Cllearworks.COH.API.Startup))]

namespace Cllearworks.COH.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            ConfigureAuth(app);
        }
    }
}
