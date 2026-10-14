using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CourseManagementMvc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TuitionFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CourseCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_CourseCategories_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "CourseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsThumbnail = table.Column<bool>(type: "bit", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseImages_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CourseCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Các khóa học phát triển web", "Lập trình Web" },
                    { 2, "Python, AI và Machine Learning", "Trí tuệ nhân tạo" },
                    { 3, "Học về hệ quản trị CSDL", "Cơ sở dữ liệu" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CourseCategoryId", "Description", "Duration", "IsActive", "Name", "TuitionFee" },
                values: new object[,]
                {
                    { 1, 1, "MVC và Entity Framework Core", 45, true, "ASP.NET Core", 4500000m },
                    { 2, 1, "Xây dựng giao diện React", 28, true, "ReactJS", 3200000m },
                    { 3, 2, "Nhập môn Python", 24, true, "Python cơ bản", 2200000m },
                    { 4, 2, "Học máy với Python", 60, false, "Machine Learning", 5800000m },
                    { 5, 3, "Truy vấn và thiết kế CSDL", 30, true, "SQL Server", 2800000m }
                });

            migrationBuilder.InsertData(
                table: "CourseImages",
                columns: new[] { "Id", "CourseId", "ImageUrl", "IsThumbnail" },
                values: new object[,]
                {
                    { 1, 1, "https://placehold.co/720x420/png?text=ASP.NET+Core", true },
                    { 2, 1, "https://placehold.co/720x420/png?text=MVC", false },
                    { 3, 2, "https://placehold.co/720x420/png?text=ReactJS", true },
                    { 4, 2, "https://placehold.co/720x420/png?text=Frontend", false },
                    { 5, 3, "https://placehold.co/720x420/png?text=Python", true },
                    { 6, 3, "https://placehold.co/720x420/png?text=Programming", false },
                    { 7, 4, "https://placehold.co/720x420/png?text=Machine+Learning", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseImages_CourseId",
                table: "CourseImages",
                column: "CourseId",
                unique: true,
                filter: "[IsThumbnail] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CourseCategoryId_Name",
                table: "Courses",
                columns: new[] { "CourseCategoryId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseImages");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "CourseCategories");
        }
    }
}
