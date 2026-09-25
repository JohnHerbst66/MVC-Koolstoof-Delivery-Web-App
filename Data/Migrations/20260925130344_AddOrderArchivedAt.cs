using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koolstoof_App_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderArchivedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Orders",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Orders");
        }
    }
}
