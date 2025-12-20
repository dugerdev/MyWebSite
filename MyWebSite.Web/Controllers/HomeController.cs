using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Interfaces;
using MyWebSite.Web.Models;
using System.Diagnostics;

namespace MyWebSite.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Test404()
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        public async Task<IActionResult> Index()
        {
            var featuredProjects = await _unitOfWork.Projects.FindAsync(p => p.IsFeatured == true);
            
            // AboutMe bilgisini al (ilk aktif kaydı al)
            var aboutMe = (await _unitOfWork.AboutMe.GetAllAsync()).FirstOrDefault();

            var viewModel = new HomeIndexViewModel
            {
                FeaturedProjects = featuredProjects,
                AboutMe = aboutMe
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var errorViewModel = new ErrorViewModel
            {
                RequestId = requestId
            };

            _logger.LogError("An error occurred. RequestId: {RequestId}", requestId);

            return View(errorViewModel);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotFound(int? statusCode = null)
        {
            statusCode ??= Response.StatusCode;

            string message = statusCode switch
            {
                404 => "The page you were looking for was not found.",
                403 => "You do not have permission to access this page.",
                500 => "A server error occurred.",
                _ => "An error occurred."
            };

            _logger.LogWarning("StatusCode {StatusCode} occurded. Path : {path}", statusCode, HttpContext.Request.Path);

            ViewBag.StatusCode = statusCode;
            ViewBag.Message = message;

            return View();
        }

        /// <summary>
        /// Privacy Policy sayfasını gösterir.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

    }
}
