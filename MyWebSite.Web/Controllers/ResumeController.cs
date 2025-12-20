using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Enums;
using MyWebSite.Core.Interfaces;
using MyWebSite.Web.Models;

namespace MyWebSite.Web.Controllers;

/// <summary>
/// Resume Controller: Public erişilebilir, yönetim Admin Dashboard'dan yapılır
/// </summary>
public class ResumeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ResumeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var allResumeItems = await _unitOfWork.ResumeItems.GetAllAsync();

        var experience = allResumeItems
            .Where(r => r.Type == ResumeItemType.Experience)
            .OrderByDescending(r => r.DisplayOrder)
            .ThenByDescending(r => r.StartDate)
            .ToList();

        var educations = allResumeItems
           .Where(r => r.Type == ResumeItemType.Education)
           .OrderByDescending(r => r.DisplayOrder)
           .ThenByDescending(r => r.StartDate)
           .ToList();

        var allSkills = await _unitOfWork.Skills.GetAllAsync();

        var professionalSkills = allSkills
            .Where(s => s.Category == SkillCategory.ProfessionalSkills)
            .OrderByDescending(s => s.DisplayOrder)
            .ToList();

        var languages = allSkills
            .Where(s => s.Category == SkillCategory.Languages)
            .OrderBy(s => s.DisplayOrder)
            .ToList();

        // ⭐ ViewModel oluştur
        var viewModel = new ResumeViewModel
        {
            Experience = experience,
            Educations = educations,
            ProfessionalSkills = professionalSkills,
            Languages = languages
        };

        return View(viewModel);
    }
}
