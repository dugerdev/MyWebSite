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
        // Dosya yükleme validasyonu: Boyut, tip ve güvenlik kontrolleri
        var (isValid, errorMessage) = ValidateImageFile(imageFile, maxSizeInMB: 5);
        if (!isValid)
        {
            ModelState.AddModelError("imageFile", errorMessage);
            return View(project);
        }

        if (ModelState.IsValid)
        {
            // Dosya yüklendiyse, proje görselini kaydet
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Benzersiz dosya adı: Guid + orijinal dosya adı (aynı isimli dosyaların çakışmasını önler)
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Veritabanında saklanacak URL (webroot'tan itibaren relative path)
                project.ImageUrl = "/images/projects/" + uniqueFileName;
            }

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project created successfully";
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

        // Dosya yükleme validasyonu
        var (isValid, errorMessage) = ValidateImageFile(imageFile, maxSizeInMB: 5);
        if (!isValid)
        {
            ModelState.AddModelError("imageFile", errorMessage);
            return View(project);
        }

        if (ModelState.IsValid)
        {
            // Entity Framework tracking: Mevcut entity'yi veritabanından al (tracked olur)
            // UpdateAsync kullanmak yerine tracked entity'nin property'lerini güncelliyoruz
            // Bu sayede "entity already tracked" hatası almayız
            var existingProject = await _unitOfWork.Projects.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            // Yeni görsel yüklendiyse, eski görseli sil ve yenisini kaydet
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Eski görseli diskten sil (depolama alanı tasarrufu için)
                if (!string.IsNullOrEmpty(existingProject.ImageUrl))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProject.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Yeni görseli kaydet
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                existingProject.ImageUrl = "/images/projects/" + uniqueFileName;
            }

            // Tracked entity'nin property'lerini güncelle
            // UpdateAsync kullanmıyoruz çünkü existingProject zaten DbContext tarafından track ediliyor
            // UpdateAsync yeni bir entity instance'ı track etmeye çalışır ve çakışma oluşturur
            existingProject.Title = project.Title;
            existingProject.Description = project.Description;
            existingProject.Technologies = project.Technologies;
            existingProject.GitHubUrl = project.GitHubUrl;
            existingProject.LiveUrl = project.LiveUrl;
            existingProject.IsFeatured = project.IsFeatured;

            // Değişiklikler tracked entity üzerinde yapıldığı için sadece SaveChanges yeterli
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

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Soft delete: Veritabanından fiziksel olarak silmez, sadece IsDeleted=true yapar
        // Bu sayede veri geri kurtarılabilir ve referans bütünlüğü korunur
        await _unitOfWork.Projects.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Project deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Yüklenen dosyanın güvenlik ve format kontrollerini yapar.
    /// Boyut, uzantı, MIME type ve dosya adı validasyonu içerir.
    /// </summary>
    private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile? file, int maxSizeInMB = 5)
    {
        // Dosya opsiyonel olduğu için yoksa geçerli kabul edilir
        if (file == null || file.Length == 0)
        {
            return (true, string.Empty);
        }

        // Dosya boyutu kontrolü: Belirtilen MB limitini aşmamalı
        var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
        if (file.Length > maxSizeInBytes)
        {
            return (false, $"File size exceeds the maximum allowed size of {maxSizeInMB} MB.");
        }

        // Dosya uzantısı kontrolü: Sadece görsel formatlarına izin verilir
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            return (false, "Only image files are allowed (JPG, JPEG, PNG, GIF, WEBP).");
        }

        // MIME type kontrolü: Dosya uzantısı ile MIME type tutarlı olmalı
        // Bu kontrol, uzantısı değiştirilmiş dosyaları yakalamak için önemlidir
        var allowedMimeTypes = new[]
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        if (string.IsNullOrEmpty(file.ContentType) || !allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return (false, "Invalid file type. Only image files are allowed.");
        }

        // Dosya adı güvenlik kontrolü: Sistem için geçersiz karakterler içermemeli
        // Path traversal saldırılarını önlemek için
        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
        {
            return (false, "File name contains invalid characters.");
        }

        return (true, string.Empty);
    }

}
