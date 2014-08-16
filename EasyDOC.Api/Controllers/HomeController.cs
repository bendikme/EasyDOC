using System.Web.Mvc;

namespace EasyDOC.Api.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}