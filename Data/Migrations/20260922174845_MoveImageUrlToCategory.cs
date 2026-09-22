using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koolstoof_App_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveImageUrlToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuCategories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuCategories");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
