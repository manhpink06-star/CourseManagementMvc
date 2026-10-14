using CourseManagementMvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseManagementMvc.ViewModels;

public class CourseIndexViewModel
{
    public string? SearchString { get; set; }
    public int? CategoryId { get; set; }
    public bool ActiveOnly { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? MaxFee { get; set; }
    public string? SortOrder { get; set; }
    public string? ErrorMessage { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
}
