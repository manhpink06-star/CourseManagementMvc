using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManagementMvc.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên khóa học.")]
    [StringLength(150)]
    [Display(Name = "Tên khóa học")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "Học phí phải lớn hơn 0.")]
    [Display(Name = "Học phí (VNĐ)")]
    public decimal TuitionFee { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0.")]
    [Display(Name = "Thời lượng (giờ)")]
    public int Duration { get; set; }

    [Display(Name = "Đang tuyển sinh")]
    public bool IsActive { get; set; } = true;

    [StringLength(2000)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại khóa học.")]
    [Display(Name = "Loại khóa học")]
    public int CourseCategoryId { get; set; }
    public CourseCategory? CourseCategory { get; set; }

    public ICollection<CourseImage> CourseImages { get; set; } = new List<CourseImage>();
}
