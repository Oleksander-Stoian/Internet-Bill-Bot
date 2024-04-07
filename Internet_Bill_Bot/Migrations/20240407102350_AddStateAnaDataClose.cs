using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internet_Bill_Bot.Migrations
{
    /// <inheritdoc />
    public partial class AddStateAnaDataClose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateClose",
                table: "Applications",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "Applications",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateClose",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "state",
                table: "Applications");
        }
    }
}
