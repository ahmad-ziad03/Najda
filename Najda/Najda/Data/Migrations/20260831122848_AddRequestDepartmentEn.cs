using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Najda.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestDepartmentEn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentEn",
                table: "Requests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentEn",
                table: "Requests");
        }
    }
}
