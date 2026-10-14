using System.ComponentModel.DataAnnotations;

namespace CourseManagementMvc.ViewModels;

public class CourseImageFormViewModel
{
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập URL hình ảnh.")]
    [Url(ErrorMessage = "URL hình ảnh không hợp lệ.")]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "URL cần bắt đầu bằng http:// hoặc https://.")]
    [StringLength(2000)]
    [Display(Name = "URL ảnh")]
    public string ImageUrl { get; set; } = string.Empty;

    [Display(Name = "Đặt làm ảnh đại diện")]
    public bool IsThumbnail { get; set; }
}
