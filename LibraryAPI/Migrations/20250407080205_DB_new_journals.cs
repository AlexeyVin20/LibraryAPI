using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class DB_new_journals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Circulation",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "PagesPerIssue",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "PublicationDate",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "Journals");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Journals",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "TargetAudience",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Publisher",
                table: "Journals",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Periodicity",
                table: "Journals",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPeerReviewed",
                table: "Journals",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexedInWebOfScience",
                table: "Journals",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexedInScopus",
                table: "Journals",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexedInRINTS",
                table: "Journals",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ISSN",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Journals",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditorInChief",
                table: "Journals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "EditorialBoard",
                table: "Journals",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Journals",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalId = table.Column<int>(type: "integer", nullable: false),
                    VolumeNumber = table.Column<int>(type: "integer", nullable: false),
                    IssueNumber = table.Column<int>(type: "integer", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PageCount = table.Column<int>(type: "integer", nullable: false),
                    Cover = table.Column<string>(type: "text", nullable: true),
                    Circulation = table.Column<int>(type: "integer", nullable: true),
                    SpecialTheme = table.Column<string>(type: "text", nullable: true),
                    ShelfId = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssueId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Authors = table.Column<List<string>>(type: "text[]", nullable: false),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    StartPage = table.Column<int>(type: "integer", nullable: false),
                    EndPage = table.Column<int>(type: "integer", nullable: false),
                    Keywords = table.Column<List<string>>(type: "text[]", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    FullText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IssueId",
                table: "Articles",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_JournalId",
                table: "Issues",
                column: "JournalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropColumn(
                name: "EditorInChief",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "EditorialBoard",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Journals");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Journals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TargetAudience",
                table: "Journals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "Journals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Publisher",
                table: "Journals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Periodicity",
                table: "Journals",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPeerReviewed",
                table: "Journals",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexedInWebOfScience",
                table: "Journals",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexedInScopus",
                table: "Journals",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexedInRINTS",
                table: "Journals",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "ISSN",
                table: "Journals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Journals",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Journals",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Circulation",
                table: "Journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Journals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PagesPerIssue",
                table: "Journals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Journals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationDate",
                table: "Journals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfId",
                table: "Journals",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
