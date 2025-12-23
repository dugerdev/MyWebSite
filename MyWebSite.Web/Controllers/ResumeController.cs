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
        // Tüm resume item'ları al ve tipine göre ayır
        var allResumeItems = await _unitOfWork.ResumeItems.GetAllAsync();

        // İş deneyimleri: DisplayOrder'a göre azalan, sonra başlangıç tarihine göre azalan sıralama
        var experience = allResumeItems
            .Where(r => r.Type == ResumeItemType.Experience)
            .OrderByDescending(r => r.DisplayOrder)
            .ThenByDescending(r => r.StartDate)
            .ToList();

        // Eğitim bilgileri: DisplayOrder'a göre azalan, sonra başlangıç tarihine göre azalan sıralama
        var educations = allResumeItems
           .Where(r => r.Type == ResumeItemType.Education)
           .OrderByDescending(r => r.DisplayOrder)
           .ThenByDescending(r => r.StartDate)
           .ToList();

        // Tüm skill'leri al ve kategoriye göre ayır
        var allSkills = await _unitOfWork.Skills.GetAllAsync();

        // Profesyonel yetenekler: DisplayOrder'a göre azalan sıralama
        var professionalSkills = allSkills
            .Where(s => s.Category == SkillCategory.ProfessionalSkills)
            .OrderByDescending(s => s.DisplayOrder)
            .ToList();

        // Diller: DisplayOrder'a göre artan sıralama (basit dillerden başlar)
        var languages = allSkills
            .Where(s => s.Category == SkillCategory.Languages)
            .OrderBy(s => s.DisplayOrder)
            .ToList();

        // ViewModel oluştur: View'a gönderilecek tüm verileri bir araya toplar
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
