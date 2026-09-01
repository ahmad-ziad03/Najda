using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Najda.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalEnglishNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityEn",
                table: "Hospitals",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Hospitals",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityEn",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Hospitals");
        }
    }
}
