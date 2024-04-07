using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internet_Bill_Bot.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProblemId",
                table: "Applications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProblemId",
                table: "Applications");
        }
    }
}
