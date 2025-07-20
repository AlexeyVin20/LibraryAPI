using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class Dialog_history3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "DialogHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalTokenCount",
                table: "DialogHistories",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "DialogHistories");

            migrationBuilder.DropColumn(
                name: "TotalTokenCount",
                table: "DialogHistories");
        }
    }
}
