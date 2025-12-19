using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Interfaces;
using MyWebSite.Web.Models;
using System.Threading.Tasks;

namespace MyWebSite.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var allProjects = await _unitOfWork.Projects.GetAllAsync();

        var allMessages = await _unitOfWork.ContactMessages.GetAllAsync();

        var unreadMessages = await _unitOfWork.ContactMessages.FindAsync(m => m.IsRead == false);

        var featuredProjects = await _unitOfWork.Projects.FindAsync(m => m.IsFeatured == true);

        var ViewModel = new DashboardViewModel
        {
            TotalProjects = allProjects.Count(),
            TotalMessages = allMessages.Count(),
            UnreadMessages = unreadMessages.Count(),
            FeaturedProjects = featuredProjects.Count(),
        };

        return View(ViewModel);
        
 
    }
}
