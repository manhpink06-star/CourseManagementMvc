using CourseManagementMvc.Data;
using CourseManagementMvc.Models;
using CourseManagementMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementMvc.Controllers;

public class CourseImagesController : AppController
{
    private readonly ApplicationDbContext _db;
    public CourseImagesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Create(int? courseId)
    {
        if (courseId is null || !await _db.Courses.AnyAsync(c => c.Id == courseId.Value))
            return Missing("CourseId không tồn tại. Không thể thêm ảnh.");

        ViewBag.CourseName = await _db.Courses
            .Where(c => c.Id == courseId.Value).Select(c => c.Name).FirstAsync();
        return View(new CourseImageFormViewModel { CourseId = courseId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseImageFormViewModel model)
    {
        if (!await _db.Courses.AnyAsync(c => c.Id == model.CourseId))
            return Missing("CourseId không tồn tại. Không thể thêm ảnh.");

        ViewBag.CourseName = await _db.Courses
            .Where(c => c.Id == model.CourseId).Select(c => c.Name).FirstAsync();

        if (!ModelState.IsValid) return View(model);

        // Dùng transaction: hủy cờ ảnh cũ TRƯỚC, rồi mới tạo ảnh mới.
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            if (model.IsThumbnail)
            {
                await _db.CourseImages
                    .Where(i => i.CourseId == model.CourseId && i.IsThumbnail)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.IsThumbnail, false));
            }

            _db.CourseImages.Add(new CourseImage
            {
                CourseId = model.CourseId,
                ImageUrl = model.ImageUrl.Trim(),
                IsThumbnail = model.IsThumbnail
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["Success"] = "Đã thêm ảnh thành công.";
            return RedirectToAction("Details", "Courses", new { id = model.CourseId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không lưu được ảnh. Vui lòng thử lại.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetThumbnail(int id)
    {
        var selected = await _db.CourseImages.FirstOrDefaultAsync(i => i.Id == id);
        if (selected is null) return Missing($"CourseImageId = {id} không tồn tại.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _db.CourseImages
                .Where(i => i.CourseId == selected.CourseId && i.Id != id && i.IsThumbnail)
                .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.IsThumbnail, false));

            selected.IsThumbnail = true;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["Success"] = "Đã đổi ảnh đại diện; ảnh cũ đã trở thành ảnh thường.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không đổi được ảnh đại diện. Vui lòng thử lại.";
        }
        return RedirectToAction("Details", "Courses", new { id = selected.CourseId });
    }
}
