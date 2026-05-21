using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardHdr_AddPartyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartyId",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_PartyId",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GateInwardHdr_PartyId",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "PartyId",
                schema: "Gate",
                table: "GateInwardHdr");
        }
    }
}
