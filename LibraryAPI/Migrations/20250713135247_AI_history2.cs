using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AI_history2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DialogHistories_Users_UserId",
                table: "DialogHistories");

            migrationBuilder.DropIndex(
                name: "IX_DialogHistories_UserId",
                table: "DialogHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DialogHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "DialogHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogHistories_UserId",
                table: "DialogHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DialogHistories_Users_UserId",
                table: "DialogHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
