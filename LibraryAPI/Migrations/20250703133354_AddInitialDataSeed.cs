using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "DateRegistered", "PasswordHash" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$12$8K8VQzjZlZBZyf8GxXQzN.LKGGdHtWxJKYf8L9PQzFqGzYtJzFqGz" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "DateRegistered", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 3, 13, 32, 18, 995, DateTimeKind.Utc).AddTicks(7107), "$2a$12$Gfzxme3NI3laNLwIt4UHi./RLFTu6iROXMnO.oMjRKo91ETP0VEpm" });
        }
    }
}
