using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Partymasterstatusidforiegnkeyadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_StatusId",
                schema: "Party",
                table: "PartyMaster",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMaster_MiscMaster_StatusId",
                schema: "Party",
                table: "PartyMaster",
                column: "StatusId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyMaster_MiscMaster_StatusId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropIndex(
                name: "IX_PartyMaster_StatusId",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
