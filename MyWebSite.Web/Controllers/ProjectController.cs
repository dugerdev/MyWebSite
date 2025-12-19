using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Interfaces;

namespace MyWebSite.Web.Controllers;

public class ProjectController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<IActionResult> Index()
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();
        return View(projects);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }
}




