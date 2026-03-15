using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Birdie69.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Questions",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "SyncedAt",
                table: "Questions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ExternalId",
                table: "Questions",
                newName: "ExternalDocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_ExternalId",
                table: "Questions",
                newName: "IX_Questions_ExternalDocumentId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Questions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Questions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "ExternalDocumentId",
                table: "Questions",
                newName: "ExternalId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Questions",
                newName: "SyncedAt");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Questions",
                newName: "Text");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_ExternalDocumentId",
                table: "Questions",
                newName: "IX_Questions_ExternalId");
        }
    }
}
