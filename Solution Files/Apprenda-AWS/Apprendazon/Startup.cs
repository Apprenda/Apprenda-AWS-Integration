using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IgniteUICSMVC5Razor.Startup))]
namespace IgniteUICSMVC5Razor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
