using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyActivityFKRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyActivityLog_PartyMaster_PartyId",
                schema: "Party",
                table: "PartyActivityLog");

            migrationBuilder.DropIndex(
                name: "IX_PartyActivityLog_PartyId",
                schema: "Party",
                table: "PartyActivityLog");

            migrationBuilder.AddColumn<int>(
                name: "PartyMasterId",
                schema: "Party",
                table: "PartyActivityLog",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyActivityLog_PartyMasterId",
                schema: "Party",
                table: "PartyActivityLog",
                column: "PartyMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyActivityLog_PartyMaster_PartyMasterId",
                schema: "Party",
                table: "PartyActivityLog",
                column: "PartyMasterId",
                principalSchema: "Party",
                principalTable: "PartyMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyActivityLog_PartyMaster_PartyMasterId",
                schema: "Party",
                table: "PartyActivityLog");

            migrationBuilder.DropIndex(
                name: "IX_PartyActivityLog_PartyMasterId",
                schema: "Party",
                table: "PartyActivityLog");

            migrationBuilder.DropColumn(
                name: "PartyMasterId",
                schema: "Party",
                table: "PartyActivityLog");

            migrationBuilder.CreateIndex(
                name: "IX_PartyActivityLog_PartyId",
                schema: "Party",
                table: "PartyActivityLog",
                column: "PartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyActivityLog_PartyMaster_PartyId",
                schema: "Party",
                table: "PartyActivityLog",
                column: "PartyId",
                principalSchema: "Party",
                principalTable: "PartyMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
