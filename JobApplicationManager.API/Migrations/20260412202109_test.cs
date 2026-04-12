using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationManager.API.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyTemplate",
                table: "CoverLetterTemplates");

            migrationBuilder.DropColumn(
                name: "FooterTemplate",
                table: "CoverLetterTemplates");

            migrationBuilder.RenameColumn(
                name: "HeaderTemplate",
                table: "CoverLetterTemplates",
                newName: "ContentTemplate");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CoverLetterTemplates",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CoverLetterTemplates");

            migrationBuilder.RenameColumn(
                name: "ContentTemplate",
                table: "CoverLetterTemplates",
                newName: "HeaderTemplate");

            migrationBuilder.AddColumn<string>(
                name: "BodyTemplate",
                table: "CoverLetterTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FooterTemplate",
                table: "CoverLetterTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
