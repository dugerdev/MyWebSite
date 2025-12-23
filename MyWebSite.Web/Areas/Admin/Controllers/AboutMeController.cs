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

        // Index view'ını göster (Edit'e yönlendirme yapma)
        return View(aboutMe);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var aboutMe = await _unitOfWork.AboutMe.GetByIdAsync(id);

        if(aboutMe == null)
        {
            return NotFound();
        }

        return View(aboutMe);
    }

    public async Task<IActionResult> Create()
    {
        // AboutMe genellikle tek bir kayıt olduğu için, zaten kayıt varsa Edit'e yönlendir
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
        // AboutMe genellikle tek bir kayıt olduğu için, zaten kayıt varsa hata ver
        var existingAboutMe = (await _unitOfWork.AboutMe.GetAllAsync()).FirstOrDefault();
        if (existingAboutMe != null)
        {
            TempData["ErrorMessage"] = "About Me information already exists. Please edit the existing record.";
            return RedirectToAction(nameof(Edit), new { id = existingAboutMe.Id });
        }

        if (ModelState.IsValid)
        {
            // Yeni entity oluştur (model binding'den gelen entity yerine)
            var newAboutMe = new AboutMeEntity
            {
                Id = Guid.NewGuid(),
                Title = aboutMe.Title,
                ShortDescription = aboutMe.ShortDescription,
                FullDescription = aboutMe.FullDescription,
                TwitterUrl = aboutMe.TwitterUrl,
                LinkedInUrl = aboutMe.LinkedInUrl,
                GitHubUrl = aboutMe.GitHubUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.AboutMe.AddAsync(newAboutMe);
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
            var existingAboutMe = await _unitOfWork.AboutMe.GetByIdAsync(id);
            if(existingAboutMe == null)
            {
                return NotFound();
            }

            // Tracked entity'nin property'lerini güncelle (UpdateAsync kullanma, zaten tracked)
            existingAboutMe.Title = aboutMe.Title;
            existingAboutMe.ShortDescription = aboutMe.ShortDescription;
            existingAboutMe.FullDescription = aboutMe.FullDescription;
            existingAboutMe.TwitterUrl = aboutMe.TwitterUrl;
            existingAboutMe.LinkedInUrl = aboutMe.LinkedInUrl;
            existingAboutMe.GitHubUrl = aboutMe.GitHubUrl;
            existingAboutMe.UpdatedDate = DateTime.UtcNow;

            // Tracked entity'de değişiklik olduğu için sadece SaveChanges yeterli
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

        await _unitOfWork.AboutMe.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "About Me information deleted successfuly!";
        return RedirectToAction(nameof(Index));
    }
}
