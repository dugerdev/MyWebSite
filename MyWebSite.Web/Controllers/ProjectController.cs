using Microsoft.AspNetCore.Mvc;

namespace MyWebSite.Web.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
