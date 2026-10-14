using CourseManagementMvc.Data;
using CourseManagementMvc.Models;
using CourseManagementMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementMvc.Controllers;

public class CourseCategoriesController : AppController
{
    private readonly ApplicationDbContext _db;
    public CourseCategoriesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var categories = await _db.CourseCategories.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new CourseCategoryListItem
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CourseCount = x.Courses.Count()
            })
            .ToListAsync();
        return View(categories);
    }

    public IActionResult Create() => View(new CourseCategory());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCategory category)
    {
        if (!string.IsNullOrWhiteSpace(category.Name))
        {
            category.Name = category.Name.Trim();
            if (await _db.CourseCategories.AnyAsync(c => c.Name == category.Name))
                ModelState.AddModelError(nameof(category.Name), "Tên loại khóa học đã tồn tại.");
        }

        if (!ModelState.IsValid) return View(category);
        _db.CourseCategories.Add(category);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã tạo loại khóa học.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không lưu được loại khóa học.");
            return View(category);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return Missing("Thiếu CourseCategoryId.");
        var category = await _db.CourseCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id.Value);
        if (category is null) return Missing($"CourseCategoryId = {id} không tồn tại.");

        ViewBag.HasCourses = await _db.Courses.AnyAsync(c => c.CourseCategoryId == id.Value);
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _db.CourseCategories.FindAsync(id);
        if (category is null) return Missing($"CourseCategoryId = {id} không tồn tại.");

        if (await _db.Courses.AnyAsync(c => c.CourseCategoryId == id))
        {
            TempData["Error"] = $"Không được xóa '{category.Name}' vì vẫn còn khóa học thuộc loại này.";
            return RedirectToAction(nameof(Index));
        }

        _db.CourseCategories.Remove(category);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa loại khóa học.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa loại khóa học vì dữ liệu đang được sử dụng.";
        }
        return RedirectToAction(nameof(Index));
    }
}
