using Microsoft.AspNetCore.Mvc;

namespace MyWebSite.Web.Controllers
{
    public class ResumeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
