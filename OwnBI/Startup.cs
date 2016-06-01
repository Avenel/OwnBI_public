using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OwnBI.Startup))]
namespace OwnBI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
