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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _unitOfWork.ContactMessages.AddAsync(model);

        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Mesajınız başarıyla gönderildi. En kısa sürede size geri dönüş yapacağız";

        return RedirectToAction("Index");
    }
}
