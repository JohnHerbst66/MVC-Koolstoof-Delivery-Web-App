using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koolstoof_App_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    SpecialPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndCondition = table.Column<int>(type: "int", nullable: false),
                    ActiveMonday = table.Column<bool>(type: "bit", nullable: false),
                    ActiveTuesday = table.Column<bool>(type: "bit", nullable: false),
                    ActiveWednesday = table.Column<bool>(type: "bit", nullable: false),
                    ActiveThursday = table.Column<bool>(type: "bit", nullable: false),
                    ActiveFriday = table.Column<bool>(type: "bit", nullable: false),
                    ActiveSaturday = table.Column<bool>(type: "bit", nullable: false),
                    ActiveSunday = table.Column<bool>(type: "bit", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEnded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Specials_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Specials_MenuItemId",
                table: "Specials",
                column: "MenuItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Specials");
        }
    }
}
