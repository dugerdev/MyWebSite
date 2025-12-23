using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;
using AboutMeEntity = MyWebSite.Core.Entities.AboutMe;

namespace MyWebSite.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles ="Admin")]
public class AboutMeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AboutMeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IActionResult> Index()
    {
        var aboutMeList = await _unitOfWork.AboutMe.GetAllAsync();
        var aboutMe = aboutMeList.FirstOrDefault();

        if(aboutMe == null)
        {
            return RedirectToAction(nameof(Create));
        }

        return View(aboutMe);
    }

    public async Task<IActionResult> Create()
    {
        // AboutMe tek bir kayıt olmalı (singleton pattern)
        // Eğer zaten bir kayıt varsa, Create yerine Edit sayfasına yönlendir
        var existingAboutMe = (await _unitOfWork.AboutMe.GetAllAsync()).FirstOrDefault();
        if (existingAboutMe != null)
        {
            TempData["InfoMessage"] = "About Me information already exists. Redirecting to edit page.";
            return RedirectToAction(nameof(Edit), new { id = existingAboutMe.Id });
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AboutMeEntity aboutMe)
    {
        // AboutMe singleton olduğu için, POST sırasında tekrar kontrol et
        // Kullanıcı Create sayfasındayken başka bir kayıt oluşturulmuş olabilir
        var existingAboutMe = (await _unitOfWork.AboutMe.GetAllAsync()).FirstOrDefault();
        if (existingAboutMe != null)
        {
            TempData["ErrorMessage"] = "About Me information already exists. Please edit the existing record.";
            return RedirectToAction(nameof(Edit), new { id = existingAboutMe.Id });
        }

        if (ModelState.IsValid)
        {
            await _unitOfWork.AboutMe.AddAsync(aboutMe);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "About Me information created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(aboutMe);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var aboutMe = await _unitOfWork.AboutMe.GetByIdAsync(id);
        
        if(aboutMe == null)
        {
            return NotFound();
        }

        return View(aboutMe);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AboutMeEntity aboutMe)
    {
        if (id != aboutMe.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Entity Framework tracking: Mevcut entity'yi veritabanından al (tracked olur)
            var existingAboutMe = await _unitOfWork.AboutMe.GetByIdAsync(id);
            if(existingAboutMe == null)
            {
                return NotFound();
            }

            // Tracked entity'nin property'lerini güncelle
            // UpdateAsync kullanmıyoruz çünkü existingAboutMe zaten DbContext tarafından track ediliyor
            existingAboutMe.Title = aboutMe.Title;
            existingAboutMe.ShortDescription = aboutMe.ShortDescription;
            existingAboutMe.FullDescription = aboutMe.FullDescription;
            existingAboutMe.TwitterUrl = aboutMe.TwitterUrl;
            existingAboutMe.LinkedInUrl = aboutMe.LinkedInUrl;
            existingAboutMe.GitHubUrl = aboutMe.GitHubUrl;

            // Değişiklikler tracked entity üzerinde yapıldığı için sadece SaveChanges yeterli
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "About Me information updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(aboutMe);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var aboutMe = await _unitOfWork.AboutMe.GetByIdAsync(id);

        if(aboutMe == null)
        {
            return NotFound();
        }

        return View(aboutMe);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var aboutMe = await _unitOfWork.AboutMe.GetByIdAsync(id);
        
        if(aboutMe == null)
        {
            return NotFound();
        }

        // Soft delete: Veritabanından fiziksel olarak silmez, sadece IsDeleted=true yapar
        await _unitOfWork.AboutMe.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "About Me information deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
