using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koolstoof_App_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekdayOpen = table.Column<TimeSpan>(type: "time", nullable: false),
                    WeekdayClose = table.Column<TimeSpan>(type: "time", nullable: false),
                    SundayOpen = table.Column<TimeSpan>(type: "time", nullable: false),
                    SundayClose = table.Column<TimeSpan>(type: "time", nullable: false),
                    AnnouncementText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnnouncementActive = table.Column<bool>(type: "bit", nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantSettings");
        }
    }
}
