using System.ComponentModel.DataAnnotations;

namespace CourseManagementMvc.Models;

public class CourseImage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập URL hình ảnh.")]
    [Url(ErrorMessage = "URL hình ảnh không hợp lệ.")]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Ảnh phải có URL bắt đầu bằng http:// hoặc https://.")]
    [StringLength(2000)]
    [Display(Name = "Đường dẫn ảnh")]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Ảnh đại diện")]
    public bool IsThumbnail { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }
}
