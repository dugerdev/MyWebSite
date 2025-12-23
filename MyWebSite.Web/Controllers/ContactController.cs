using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;

namespace MyWebSite.Web.Controllers;

public class ContactController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactMessage model)
    {
        // Model validasyonu: FluentValidation kuralları kontrol edilir
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // İletişim mesajını veritabanına kaydet
        await _unitOfWork.ContactMessages.AddAsync(model);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your message has been sent successfully. We will get back to you as soon as possible.";

        // PRG Pattern (Post-Redirect-Get): Form gönderiminden sonra GET isteği yapılır
        // Bu sayede kullanıcı sayfayı yenilediğinde form tekrar gönderilmez
        return RedirectToAction("Index");
    }
}
