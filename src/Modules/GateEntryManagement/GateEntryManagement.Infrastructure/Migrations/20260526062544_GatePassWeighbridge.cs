using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GatePassWeighbridge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                schema: "Gate",
                table: "GatePassHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                schema: "Gate",
                table: "GatePassHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TareWeight",
                schema: "Gate",
                table: "GatePassHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrossWeight",
                schema: "Gate",
                table: "GatePassHdr");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                schema: "Gate",
                table: "GatePassHdr");

            migrationBuilder.DropColumn(
                name: "TareWeight",
                schema: "Gate",
                table: "GatePassHdr");
        }
    }
}
