using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(EasyDOC.Api.Controllers.Startup))]
namespace EasyDOC.Api.Controllers
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}