using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;
using System.Threading.Tasks;

namespace MyWebSite.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProjectsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProjectsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }
    public async Task<IActionResult> Index()
    {
        var allProjects = await _unitOfWork.Projects.GetAllAsync();
        return View(allProjects);
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

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                project.ImageUrl = "/images/projects/" + uniqueFileName;
            }

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project created successfuly";
            return RedirectToAction(nameof(Index));
        }

        return View(project);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Project project, IFormFile? imageFile)
    {
        if (id != project.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Tracked entity'yi al
            var existingProject = await _unitOfWork.Projects.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            // Eğer yeni resim yüklendiyse
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Eski resmi sil (eğer varsa)
                if (!string.IsNullOrEmpty(existingProject.ImageUrl))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProject.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Yeni resmi kaydet
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                existingProject.ImageUrl = "/images/projects/" + uniqueFileName;
            }

            // Tracked entity'nin property'lerini güncelle (UpdateAsync kullanma, zaten tracked)
            existingProject.Title = project.Title;
            existingProject.Description = project.Description;
            existingProject.Technologies = project.Technologies;
            existingProject.GitHubUrl = project.GitHubUrl;
            existingProject.LiveUrl = project.LiveUrl;
            existingProject.IsFeatured = project.IsFeatured;

            // Tracked entity'de değişiklik olduğu için sadece SaveChanges yeterli
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(project);
    }
    public async Task<IActionResult> Delete(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    // POST: Delete project
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Soft delete (IsDeleted = true)
        await _unitOfWork.Projects.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Project deleted successfully!";
        return RedirectToAction(nameof(Index));
    }


}
