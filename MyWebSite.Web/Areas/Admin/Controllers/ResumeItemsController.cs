using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;

namespace MyWebSite.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ResumeItemsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ResumeItemsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var allResumeItems = await _unitOfWork.ResumeItems.GetAllAsync();
        return View(allResumeItems);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var resumeItem = await _unitOfWork.ResumeItems.GetByIdAsync(id);

        if(resumeItem == null)
        {
            return NotFound();
        }

        return View(resumeItem);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResumeItem resumeItem)
    {
        if (ModelState.IsValid)
        {
            await _unitOfWork.ResumeItems.AddAsync(resumeItem);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Resume item created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(resumeItem);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var resumeItem = await _unitOfWork.ResumeItems.GetByIdAsync(id);
        if (resumeItem == null)
        {
            return NotFound();
        }

        return View(resumeItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ResumeItem resumeItem)
    {
        if (id != resumeItem.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Tracked entity'yi al
            var existingResumeItem = await _unitOfWork.ResumeItems.GetByIdAsync(id);
            if (existingResumeItem == null)
            {
                return NotFound();
            }

            // Tracked entity'nin property'lerini güncelle (UpdateAsync kullanma, zaten tracked)
            existingResumeItem.Title = resumeItem.Title;
            existingResumeItem.CompanyOrInstitution = resumeItem.CompanyOrInstitution;
            existingResumeItem.Location = resumeItem.Location;
            existingResumeItem.StartDate = resumeItem.StartDate;
            existingResumeItem.EndDate = resumeItem.EndDate;
            existingResumeItem.Description = resumeItem.Description;
            existingResumeItem.Type = resumeItem.Type;
            existingResumeItem.DisplayOrder = resumeItem.DisplayOrder;

            // Tracked entity'de değişiklik olduğu için sadece SaveChanges yeterli
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Resume item updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(resumeItem);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var resumeItem = await _unitOfWork.ResumeItems.GetByIdAsync(id);
        if (resumeItem == null)
        {
            return NotFound();
        }
        return View(resumeItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var resumeItem = await _unitOfWork.ResumeItems.GetByIdAsync(id);
        if(resumeItem == null)
        {
            return NotFound();
        }

        await _unitOfWork.ResumeItems.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Resume item deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

