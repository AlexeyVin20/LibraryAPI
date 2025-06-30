using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBookInstanceToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookInstanceId",
                table: "Reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookInstanceId",
                table: "Reservations",
                column: "BookInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_BookInstances_BookInstanceId",
                table: "Reservations",
                column: "BookInstanceId",
                principalTable: "BookInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_BookInstances_BookInstanceId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BookInstanceId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "BookInstanceId",
                table: "Reservations");
        }
    }
}
