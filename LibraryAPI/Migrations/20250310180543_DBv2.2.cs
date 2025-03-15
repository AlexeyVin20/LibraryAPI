using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class DBv22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Journals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ISSN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    Periodicity = table.Column<int>(type: "integer", nullable: false),
                    PagesPerIssue = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Publisher = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FoundationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Circulation = table.Column<int>(type: "integer", nullable: false),
                    IsOpenAccess = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    TargetAudience = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsPeerReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    IsIndexedInRINTS = table.Column<bool>(type: "boolean", nullable: false),
                    IsIndexedInScopus = table.Column<bool>(type: "boolean", nullable: false),
                    IsIndexedInWebOfScience = table.Column<bool>(type: "boolean", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PageCount = table.Column<int>(type: "integer", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journals", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Journals");
        }
    }
}
