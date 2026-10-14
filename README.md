# ÔN THI GIỮA KỲ – QUẢN LÝ KHÓA HỌC (ASP.NET CORE MVC + EF CORE)

Project mẫu thực hành đúng trọng tâm đề. Có 3 model, SQL Server LocalDB, quan hệ 1-n, 5 khóa học, 3 loại, 7 ảnh, Create/Edit, tìm kiếm-lọc-sắp xếp, đổi ảnh đại diện và chặn xóa loại đang dùng.

## I. Cách chạy trên Visual Studio 2022 (máy Windows)

### Bước 1. Mở project

1. Giải nén ZIP (không mở trực tiếp bên trong ZIP).
2. Mở Visual Studio 2022, chọn **Open a project or solution**.
3. Chọn file `CourseManagementMvc.sln` (hoặc `CourseManagementMvc.csproj`).
4. Nếu được hỏi, đồng ý Restore NuGet packages.
5. Máy cần cài workload **ASP.NET and web development**, **.NET 8 SDK**, và **SQL Server Express LocalDB** (thường có qua Visual Studio Installer).

### Bước 2. Kiểm tra kết nối database

File `appsettings.json` mặc định:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CourseManagementMidterm;Trusted_Connection=True;TrustServerCertificate=True"
```

Nếu bạn dùng SQL Server trên máy khác / instance khác, thay `Server=...` cho đúng. Ví dụ SQL Server instance cài tại máy:

```json
"DefaultConnection": "Server=.;Database=CourseManagementMidterm;Trusted_Connection=True;TrustServerCertificate=True"
```

### Bước 3. Tạo database bằng EF Core Migration

Trong Visual Studio mở **Tools > NuGet Package Manager > Package Manager Console**.

Chọn **Default project: CourseManagementMvc**. Lần lượt gõ:

```powershell
Add-Migration InitialCreate
Update-Database
```

Nếu không có lệnh `Add-Migration`, kiểm tra NuGet restore và đảm bảo `Microsoft.EntityFrameworkCore.Tools` được cài; thử Build trước khi chạy lệnh.

**Lưu ý quan trọng:** Chỉ chạy `Add-Migration InitialCreate` **một lần** trong một project. Những lần chạy lại sau đó chỉ cần `Update-Database`. Nếu project đã có migration tên `InitialCreate`, không tạo lại.

Có thể dùng Terminal tương đương:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Nếu dotnet-ef đã cài, dùng `dotnet tool update --global dotnet-ef --version 8.0.11` thay cho install. Tất cả command terminal chạy trong thư mục có `.csproj`.

### Bước 4. Kiểm tra trong SQL Server Object Explorer

Trong Visual Studio: **View > SQL Server Object Explorer**. Mở `(localdb)\\MSSQLLocalDB` > Databases > `CourseManagementMidterm` > Tables.

Phải thấy 3 bảng:

- `dbo.CourseCategories`
- `dbo.Courses`
- `dbo.CourseImages`

Bảng `__EFMigrationsHistory` là bảng do EF tạo, không thuộc ba bảng nghiệp vụ.

### Bước 5. Chạy

Nhấn **Ctrl+F5** (hoặc F5). Ứng dụng mở trang `/Courses/Index`.

Các đường dẫn:

- `/Courses` — danh sách và lọc.
- `/Courses/Create` — thêm khóa học.
- `/Courses/Details/1` — chi tiết và quản lý ảnh.
- `/Courses/Edit/1` — sửa khóa học.
- `/CourseImages/Create?courseId=1` — thêm URL ảnh.
- `/CourseCategories` — xem loại khóa học và thử xóa.

## II. Cấu trúc code

```text
CourseManagementMvc/
├── CourseManagementMvc.sln
├── CourseManagementMvc.csproj
├── appsettings.json
├── Program.cs
├── Models/
│   ├── Course.cs
│   ├── CourseCategory.cs
│   └── CourseImage.cs
├── Data/
│   └── ApplicationDbContext.cs
├── ViewModels/
│   ├── CourseFormViewModel.cs
│   ├── CourseIndexViewModel.cs
│   ├── CourseImageFormViewModel.cs
│   └── CourseCategoryListItem.cs
├── Controllers/
│   ├── CoursesController.cs
│   ├── CourseImagesController.cs
│   ├── CourseCategoriesController.cs
│   ├── AppController.cs
│   └── ErrorController.cs
├── Views/ (Razor giao diện)
└── wwwroot/css/site.css
```

## III. Bảng đối chiếu yêu cầu đề thi

| Yêu cầu | File / vị trí |
|---|---|
| Model + validation | `Models/*.cs` và `ViewModels/*.cs` |
| Quan hệ Category-Course-Image | `Data/ApplicationDbContext.cs` → `OnModelCreating()` |
| Database 5 khóa học / 3 loại / 7 ảnh | `Data/ApplicationDbContext.cs` → `HasData()` |
| Chọn loại từ database khi Create/Edit | `CoursesController.FillCategoriesAsync()` + `_CourseForm.cshtml` |
| Tìm theo tên | `CoursesController.Index()` → `Name.Contains(keyword)` |
| Lọc loại | `Where(c => c.CourseCategoryId == categoryId.Value)` |
| Lọc trạng thái | `Where(c => c.IsActive)` |
| Học phí từ/đến | `Where(c => c.TuitionFee >=...)`, `<=...` |
| 3 chế độ sắp xếp | `switch sortOrder` trong `CoursesController.Index()` |
| Giữ bộ lọc khi submit | `CourseIndexViewModel` + `Views/Courses/Index.cshtml` |
| Ngắn hạn/dài hạn, không tạo DB field | `Views/Courses/Index.cshtml` dùng điều kiện `Duration <= 30` |
| Nhiều ảnh, Detail hiển thị tất cả | `CoursesController.Details()` → `Include(c => c.CourseImages)` |
| Thêm ảnh URL | `CourseImagesController.Create()` |
| Chọn/đổi ảnh đại diện | `CourseImagesController.SetThumbnail()` và `Create()` transaction |
| Tối đa 1 thumbnail | unique filtered index trong `ApplicationDbContext` |
| Cấm xóa Category có Course | `CourseCategoriesController.DeleteConfirmed()` + `DeleteBehavior.Restrict` |
| Không mất Course ngừng tuyển sinh | Index chỉ lọc active nếu checkbox `activeOnly=true` |
| ID không tồn tại | `AppController.Missing()` → trang thân thiện 404 |
| Bộ lọc sai / không có kết quả | `CoursesController.Index()` + giao diện |
| Create/Edit dữ liệu sai | `ModelState.IsValid`, `[Required]`, `[Range]`, lỗi nghiệp vụ |

## IV. Dữ liệu mẫu để demo

| ID | Khóa học | Loại | Học phí (đ) | Giờ | Trạng thái | Ảnh |
|---|---|---|---:|---:|---|---:|
| 1 | ASP.NET Core | Lập trình Web | 4.500.000 | 45 | Đang tuyển sinh | 2 |
| 2 | ReactJS | Lập trình Web | 3.200.000 | 28 | Đang tuyển sinh | 2 |
| 3 | Python cơ bản | Trí tuệ nhân tạo | 2.200.000 | 24 | Đang tuyển sinh | 2 |
| 4 | Machine Learning | Trí tuệ nhân tạo | 5.800.000 | 60 | Ngừng tuyển sinh | 1 |
| 5 | SQL Server | Cơ sở dữ liệu | 2.800.000 | 30 | Đang tuyển sinh | 0 |

Course ID 5 cố ý chưa có ảnh để demo phần xử lý ngoại lệ. Có thể thêm ảnh trong lúc trình bày.

## V. Kịch bản 14 mục demo (theo thứ tự)

1. Mở SQL Server Object Explorer → 3 bảng (Courses, CourseCategories, CourseImages).
2. Mở `ApplicationDbContext.cs` → giải thích `HasOne/WithMany/HasForeignKey` và khóa ngoại trong database.
3. Mở `/Courses` → thấy 5 khóa, tên loại và trạng thái.
4. Gõ `ASP` → thấy ASP.NET Core.
5. Chọn **Lập trình Web** → thấy ASP.NET Core, ReactJS.
6. Tích **Chỉ đang tuyển sinh** → không thấy Machine Learning.
7. Nhập học phí từ `3000000` đến `5000000` → thấy ASP.NET Core và ReactJS nếu không có các lọc khác.
8. Chọn sắp xếp **Học phí giảm dần**, rồi **Thời lượng tăng dần**.
9. Chỉ vào nhãn: ReactJS 28 giờ = Ngắn hạn; ASP.NET Core 45 giờ = Dài hạn.
10. `/Courses/Create`: nhập đầy đủ khóa học mới, lưu. Thử nhập học phí `0` hoặc tên trùng để kiểm tra validation. `/Courses/Edit/1`: thử chuyển IsActive.
11. `/Courses/Details/1` → **Thêm ảnh bằng URL** (vd `https://placehold.co/720x420/png?text=Anh+Moi`).
12. Trong Details, nhấn **Chọn làm ảnh đại diện** ở ảnh thường → ảnh cũ trở thành ảnh thường.
13. `/CourseCategories` → **Thử xóa** Lập trình Web → xác nhận → nhận thông báo không thể xóa.
14. Gõ trực tiếp `/Courses/Details/99999` → trang thông báo 404 thân thiện.

**Tip:** Sau khi thực hiện từng demo, bấm **Xóa lọc** để tránh nhầm kết quả lọc ở bước tiếp theo. `courseId=5` không có ảnh vẫn phải hiển thị trang Details bình thường.

## VI. Câu hỏi vấn đáp thường gặp

**1. Vì sao dùng `IQueryable`?**
Vì điều kiện `Where`, `OrderBy` sẽ được EF Core chuyển thành truy vấn SQL, xử lý trong database. Chỉ gọi `ToListAsync()` sau khi ghép xong bộ lọc, đúng yêu cầu không lấy hết dữ liệu về lọc bằng vòng lặp.

**2. Khác nhau giữa `Include` và `HasOne`?**
`HasOne(...).WithMany(...)` khai báo quan hệ trong model database. `Include(...)` yêu cầu lấy thêm navigation data khi truy vấn để hiển thị tên loại và ảnh.

**3. Phân loại thời lượng nằm ở đâu?**
Ở giao diện Razor: `Duration <= 30 ? "Ngắn hạn" : "Dài hạn"`. Không thêm cột mới trong database.

**4. Khi nào kiểm tra `ModelState.IsValid`?**
Trong POST Create/Edit/Thêm ảnh sau khi MVC bind dữ liệu. Nếu false, trả lại View kèm lỗi; không lưu database.

**5. Vì sao phải dùng cả `AnyAsync` và unique index?**
`AnyAsync` để báo lỗi dễ hiểu. Unique index trong DB chống dữ liệu trùng nếu nhiều request cùng đến đồng thời. Trong `Course`, index `(CourseCategoryId, Name)` cho phép cùng tên ở hai category khác nhau.

**6. Tại sao tối đa 1 ảnh đại diện?**
Đổi ảnh bằng transaction: bỏ cờ thumbnail cũ rồi bật cờ ảnh mới. Database còn có unique filtered index trên `CourseId` với `IsThumbnail=1` để ngăn tồn tại hai thumbnail trong cùng course.

**7. Nếu xóa Category đang có Course?**
Kiểm tra `_db.Courses.AnyAsync(...)`. Nếu còn Course thì từ chối; quan hệ dùng `DeleteBehavior.Restrict` bảo vệ thêm ở tầng database.

**8. `IsActive=false` có bị xóa khỏi danh sách không?**
Không. Chỉ hiển thị "Ngừng tuyển sinh". Chỉ `activeOnly=true` mới thêm điều kiện `Where(c => c.IsActive)`.

**9. `CourseCategoryId` không tồn tại thì sao?**
Controller kiểm tra `AnyAsync` và `ModelState.AddModelError`, đồng thời FK ở DB bảo vệ ràng buộc.

**10. ID không tồn tại?**
`FirstOrDefaultAsync`/`FindAsync` trả `null`; hệ thống hiển thị trang 404 thay vì throw exception.

## VII. Bài luyện tốc độ trong 45–50 phút

- Phút 0–8: cấu hình project, models, DbContext, migrations, chạy DB.
- Phút 8–22: Index + 5 bộ lọc + 3 sắp xếp, hiển thị Ngắn/Dài.
- Phút 22–32: Create/Edit + validation + dropdown.
- Phút 32–42: Image Create + SetThumbnail + Details.
- Phút 42–50: chống xóa Category, kiểm lỗi ID/không có ảnh, demo.

Nếu có project ORM trên lớp chạy sẵn, ưu tiên dùng lại để tiết kiệm thời gian. **Đừng chỉ học thuộc**: hãy thử đổi điều kiện lọc, tên cột, thêm một ảnh, tự sửa một validation mà không nhìn đáp án.

## VIII. Một số lỗi thường gặp

- **`Add-Migration` is not recognized**: Restore NuGet, chắc chắn `.Tools` đã cài, Build trước, chọn đúng Default project.
- **`Unable to connect to SQL Server`**: Kiểm tra LocalDB có cài không, chuỗi kết nối trong appsettings.
- **`Invalid object name 'Courses'`**: Chưa chạy `Update-Database` hoặc sai tên database.
- **`A database operation failed`**: Nếu chỉnh model sau migration, tạo migration tiếp bằng `Add-Migration TenMoi` rồi `Update-Database` (không tạo lại InitialCreate).
- **Chỉ hiện trang mặc định**: kiểm tra `Program.cs` route mặc định là `Courses/Index`.
- **Chưa có ảnh**: trang Details sẽ hiện thông báo và cho phép thêm ảnh; không được gọi `.First()` với danh sách rỗng.
- **Ảnh ngoài bị lỗi**: kiểm tra URL mở trực tiếp được trên browser và máy có mạng. Ứng dụng chỉ lưu URL, không tải file ảnh về máy.

## Chú ý

Mẫu này viết để thực hành, chưa có authentication/authorization. Nếu triển khai Internet, cần bổ sung đăng nhập, phân quyền admin, giới hạn thao tác sửa/xóa, logging và kiểm tra URL ảnh an toàn hơn. Bản mẫu chưa được biên dịch ngay trong môi trường tạo file; hãy thực hiện Restore/Build bằng Visual Studio rồi chạy thử theo hướng dẫn.
