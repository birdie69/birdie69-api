using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Birdie69.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndCouples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Couples_Users_InitiatorId",
                table: "Couples",
                column: "InitiatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Couples_Users_PartnerId",
                table: "Couples",
                column: "PartnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Couples_Users_InitiatorId",
                table: "Couples");

            migrationBuilder.DropForeignKey(
                name: "FK_Couples_Users_PartnerId",
                table: "Couples");
        }
    }
}
