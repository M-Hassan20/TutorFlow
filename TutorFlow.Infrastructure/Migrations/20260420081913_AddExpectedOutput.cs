using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedOutput",
                table: "Assignments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedOutput",
                table: "Assignments");
        }
    }
}
