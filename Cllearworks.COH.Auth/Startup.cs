using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Cllearworks.COH.Auth.Startup))]
namespace Cllearworks.COH.Auth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            ConfigureAuth(app);
        }
    }
}
