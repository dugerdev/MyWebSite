using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Interfaces;
using System.Diagnostics;

namespace MyWebSite.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProjects = await _unitOfWork.Projects.FindAsync(p => p.IsFeatured == true);

            return View(featuredProjects);
        }

    }
}
