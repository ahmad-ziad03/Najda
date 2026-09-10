using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Najda.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDonorIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Donors",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Donors");
        }
    }
}
