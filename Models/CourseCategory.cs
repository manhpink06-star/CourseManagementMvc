using System.ComponentModel.DataAnnotations;

namespace CourseManagementMvc.Models;

public class CourseCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên loại khóa học.")]
    [StringLength(100)]
    [Display(Name = "Loại khóa học")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
