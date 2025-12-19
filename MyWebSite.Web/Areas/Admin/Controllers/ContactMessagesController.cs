using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Core.Entities;
using MyWebSite.Core.Interfaces;

namespace MyWebSite.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContactMessagesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactMessagesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: List all contact messages
    public async Task<IActionResult> Index()
    {
        var messages = await _unitOfWork.ContactMessages.GetAllAsync();
        // En yeni mesajlar üstte görünsün
        var orderedMessages = messages.OrderByDescending(m => m.CreatedAt);
        return View(orderedMessages);
    }

    // GET: Message details
    public async Task<IActionResult> Details(Guid id)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        // Mesajı okundu olarak işaretle (eğer okunmamışsa)
        if (!message.IsRead)
        {
            message.IsRead = true;
            await _unitOfWork.ContactMessages.UpdateAsync(message);
            await _unitOfWork.SaveChangesAsync();
        }

        return View(message);
    }

    // POST: Mark message as read
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        message.IsRead = true;
        await _unitOfWork.ContactMessages.UpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Message marked as read.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Delete confirmation
    public async Task<IActionResult> Delete(Guid id)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
        if (message == null)
        {
            return NotFound();
        }
        return View(message);
    }

    // POST: Delete message
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        // Soft delete
        await _unitOfWork.ContactMessages.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Message deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

