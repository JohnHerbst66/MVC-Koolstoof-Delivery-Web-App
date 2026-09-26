using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Koolstoof_App_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Choices",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OptionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionGroups_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionGroupId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionChoices_OptionGroups_OptionGroupId",
                        column: x => x.OptionGroupId,
                        principalTable: "OptionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionChoices_OptionGroupId",
                table: "OptionChoices",
                column: "OptionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionGroups_MenuItemId",
                table: "OptionGroups",
                column: "MenuItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionChoices");

            migrationBuilder.DropTable(
                name: "OptionGroups");

            migrationBuilder.DropColumn(
                name: "Choices",
                table: "OrderItems");
        }
    }
}
