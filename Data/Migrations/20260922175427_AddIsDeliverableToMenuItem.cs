using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koolstoof_App_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeliverableToMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeliverable",
                table: "MenuItems",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeliverable",
                table: "MenuItems");
        }
    }
}
