using CourseManagementMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementMvc.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseImage> CourseImages => Set<CourseImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.CourseCategory)
            .WithMany(category => category.Courses)
            .HasForeignKey(c => c.CourseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseImage>()
            .HasOne(i => i.Course)
            .WithMany(c => c.CourseImages)
            .HasForeignKey(i => i.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tên khóa học chỉ được xuất hiện một lần trong cùng một loại.
        modelBuilder.Entity<Course>()
            .HasIndex(c => new { c.CourseCategoryId, c.Name })
            .IsUnique();

        // SQL Server: chỉ tối đa 1 ảnh đại diện trên mỗi khóa học.
        modelBuilder.Entity<CourseImage>()
            .HasIndex(i => i.CourseId)
            .IsUnique()
            .HasFilter("[IsThumbnail] = 1");

        modelBuilder.Entity<CourseCategory>().HasData(
            new CourseCategory { Id = 1, Name = "Lập trình Web", Description = "Các khóa học phát triển web" },
            new CourseCategory { Id = 2, Name = "Trí tuệ nhân tạo", Description = "Python, AI và Machine Learning" },
            new CourseCategory { Id = 3, Name = "Cơ sở dữ liệu", Description = "Học về hệ quản trị CSDL" }
        );

        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, Name = "ASP.NET Core", TuitionFee = 4500000m, Duration = 45, IsActive = true, Description = "MVC và Entity Framework Core", CourseCategoryId = 1 },
            new Course { Id = 2, Name = "ReactJS", TuitionFee = 3200000m, Duration = 28, IsActive = true, Description = "Xây dựng giao diện React", CourseCategoryId = 1 },
            new Course { Id = 3, Name = "Python cơ bản", TuitionFee = 2200000m, Duration = 24, IsActive = true, Description = "Nhập môn Python", CourseCategoryId = 2 },
            new Course { Id = 4, Name = "Machine Learning", TuitionFee = 5800000m, Duration = 60, IsActive = false, Description = "Học máy với Python", CourseCategoryId = 2 },
            new Course { Id = 5, Name = "SQL Server", TuitionFee = 2800000m, Duration = 30, IsActive = true, Description = "Truy vấn và thiết kế CSDL", CourseCategoryId = 3 }
        );

        // Ảnh minh họa từ URL: nếu ảnh ngoài không tải được, UI vẫn có xử lý ảnh lỗi.
        modelBuilder.Entity<CourseImage>().HasData(
            new CourseImage { Id = 1, CourseId = 1, ImageUrl = "https://placehold.co/720x420/png?text=ASP.NET+Core", IsThumbnail = true },
            new CourseImage { Id = 2, CourseId = 1, ImageUrl = "https://placehold.co/720x420/png?text=MVC", IsThumbnail = false },
            new CourseImage { Id = 3, CourseId = 2, ImageUrl = "https://placehold.co/720x420/png?text=ReactJS", IsThumbnail = true },
            new CourseImage { Id = 4, CourseId = 2, ImageUrl = "https://placehold.co/720x420/png?text=Frontend", IsThumbnail = false },
            new CourseImage { Id = 5, CourseId = 3, ImageUrl = "https://placehold.co/720x420/png?text=Python", IsThumbnail = true },
            new CourseImage { Id = 6, CourseId = 3, ImageUrl = "https://placehold.co/720x420/png?text=Programming", IsThumbnail = false },
            new CourseImage { Id = 7, CourseId = 4, ImageUrl = "https://placehold.co/720x420/png?text=Machine+Learning", IsThumbnail = true }
            // Khóa SQL Server (ID=5) cố ý chưa có ảnh để kiểm thử trường hợp không có ảnh.
        );
    }
}
