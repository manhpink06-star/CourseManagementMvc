using CourseManagementMvc.Data;
using CourseManagementMvc.Models;
using CourseManagementMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementMvc.Controllers;

public class CoursesController : AppController
{
    private readonly ApplicationDbContext _db;

    public CoursesController(ApplicationDbContext db) => _db = db;

    // TẤT CẢ điều kiện lọc được ghép thành SQL, không ToList() trước khi Where().
    public async Task<IActionResult> Index(
        string? searchString, int? categoryId, bool activeOnly = false,
        decimal? minFee = null, decimal? maxFee = null, string? sortOrder = null)
    {
        var vm = new CourseIndexViewModel
        {
            SearchString = searchString,
            CategoryId = categoryId,
            ActiveOnly = activeOnly,
            MinFee = minFee,
            MaxFee = maxFee,
            SortOrder = sortOrder
        };
        vm.Categories = await _db.CourseCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == categoryId
            }).ToListAsync();

        if (minFee.HasValue && maxFee.HasValue && minFee > maxFee)
        {
            vm.ErrorMessage = "Học phí tối thiểu không được lớn hơn học phí tối đa.";
            return View(vm);
        }
        if (minFee < 0 || maxFee < 0)
        {
            vm.ErrorMessage = "Khoảng học phí không được âm.";
            return View(vm);
        }
        if (categoryId.HasValue &&
            !await _db.CourseCategories.AnyAsync(c => c.Id == categoryId.Value))
        {
            vm.ErrorMessage = "Loại khóa học được lọc không tồn tại.";
            return View(vm);
        }
        if (!ModelState.IsValid)
        {
            vm.ErrorMessage = "Bộ lọc có giá trị không hợp lệ, vui lòng kiểm tra lại.";
            return View(vm);
        }

        IQueryable<Course> query = _db.Courses.AsNoTracking()
            .Include(c => c.CourseCategory);

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            string keyword = searchString.Trim();
            query = query.Where(c => c.Name.Contains(keyword));
        }
        if (categoryId.HasValue)
            query = query.Where(c => c.CourseCategoryId == categoryId.Value);
        if (activeOnly)
            query = query.Where(c => c.IsActive);
        if (minFee.HasValue)
            query = query.Where(c => c.TuitionFee >= minFee.Value);
        if (maxFee.HasValue)
            query = query.Where(c => c.TuitionFee <= maxFee.Value);

        query = sortOrder switch
        {
            "fee_asc" => query.OrderBy(c => c.TuitionFee).ThenBy(c => c.Id),
            "fee_desc" => query.OrderByDescending(c => c.TuitionFee).ThenBy(c => c.Id),
            "duration_asc" => query.OrderBy(c => c.Duration).ThenBy(c => c.Id),
            _ => query.OrderBy(c => c.Id)
        };

        vm.Courses = await query.ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return Missing("Chưa cung cấp CourseId.");
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.CourseCategory)
            .Include(c => c.CourseImages)
            .FirstOrDefaultAsync(c => c.Id == id.Value);

        if (course is null) return Missing($"Không tìm thấy khóa học có ID = {id}.");
        return View(course);
    }

    public async Task<IActionResult> Create()
        => View(await FillCategoriesAsync(new CourseFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        await ValidateCategoryAndNameAsync(model);
        if (!ModelState.IsValid) return View(await FillCategoriesAsync(model));

        var course = new Course();
        CopyToCourse(model, course);
        _db.Courses.Add(course);

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã tạo khóa học thành công.";
            return RedirectToAction(nameof(Details), new { id = course.Id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "Không thể lưu. Tên khóa học có thể bị trùng trong cùng loại.");
            return View(await FillCategoriesAsync(model));
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return Missing("Chưa cung cấp CourseId.");
        var c = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.Value);
        if (c is null) return Missing($"Không tìm thấy khóa học ID = {id}.");

        var model = new CourseFormViewModel
        {
            Id = c.Id, Name = c.Name, TuitionFee = c.TuitionFee,
            Duration = c.Duration, IsActive = c.IsActive,
            Description = c.Description, CourseCategoryId = c.CourseCategoryId
        };
        return View(await FillCategoriesAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel model)
    {
        if (id != model.Id) return Missing("ID khóa học không khớp.");
        var course = await _db.Courses.FindAsync(id);
        if (course is null) return Missing($"Không tìm thấy khóa học ID = {id}.");

        await ValidateCategoryAndNameAsync(model, id);
        if (!ModelState.IsValid) return View(await FillCategoriesAsync(model));

        CopyToCourse(model, course);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật khóa học thành công.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "Không thể lưu. Tên khóa học có thể bị trùng trong cùng loại.");
            return View(await FillCategoriesAsync(model));
        }
    }

    private async Task ValidateCategoryAndNameAsync(CourseFormViewModel model, int? exceptId = null)
    {
        if (!await _db.CourseCategories.AnyAsync(x => x.Id == model.CourseCategoryId))
            ModelState.AddModelError(nameof(model.CourseCategoryId), "Loại khóa học không tồn tại trong database.");

        if (!string.IsNullOrWhiteSpace(model.Name))
        {
            model.Name = model.Name.Trim();
            bool duplicate = await _db.Courses.AnyAsync(x =>
                x.CourseCategoryId == model.CourseCategoryId &&
                x.Name == model.Name &&
                (!exceptId.HasValue || x.Id != exceptId.Value));
            if (duplicate)
                ModelState.AddModelError(nameof(model.Name), "Tên khóa học đã tồn tại trong loại này.");
        }
    }

    private async Task<CourseFormViewModel> FillCategoriesAsync(CourseFormViewModel model)
    {
        model.Categories = await _db.CourseCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(), Text = c.Name
            }).ToListAsync();
        return model;
    }

    private static void CopyToCourse(CourseFormViewModel form, Course c)
    {
        c.Name = form.Name.Trim();
        c.TuitionFee = form.TuitionFee;
        c.Duration = form.Duration;
        c.IsActive = form.IsActive;
        c.Description = form.Description?.Trim();
        c.CourseCategoryId = form.CourseCategoryId;
    }
}
