using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VMSS.Web.Startup))]
namespace VMSS.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
