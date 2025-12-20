using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;

namespace MyWebSite.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SkillsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public SkillsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var allSkills = await _unitOfWork.Skills.GetAllAsync();
        return View(allSkills);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var skill = await _unitOfWork.Skills.GetByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }

        return View(skill);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Skill skill)
    {
        if (ModelState.IsValid)
        {
            await _unitOfWork.Skills.AddAsync(skill);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Skill created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(skill);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var skill = await _unitOfWork.Skills.GetByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }

        return View(skill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Skill skill)
    {
        if (id != skill.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Tracked entity'yi al
            var existingSkill = await _unitOfWork.Skills.GetByIdAsync(id);
            if (existingSkill == null)
            {
                return NotFound();
            }

            // Tracked entity'nin property'lerini güncelle (UpdateAsync kullanma, zaten tracked)
            existingSkill.Name = skill.Name;
            existingSkill.Category = skill.Category;
            existingSkill.DisplayOrder = skill.DisplayOrder;

            // Tracked entity'de değişiklik olduğu için sadece SaveChanges yeterli
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Skill updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(skill);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var skill = await _unitOfWork.Skills.GetByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }
        return View(skill);
    }

    // POST: Delete skill
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var skill = await _unitOfWork.Skills.GetByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }

        // Soft delete (IsDeleted = true)
        await _unitOfWork.Skills.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Skill deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

