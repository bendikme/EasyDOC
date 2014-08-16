using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EasyDOC.Api.App_Start;
using WebMatrix.WebData;

namespace EasyDOC.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebSecurity.InitializeDatabaseConnection("ManualContext", "User", "Id", "UserName", true);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}