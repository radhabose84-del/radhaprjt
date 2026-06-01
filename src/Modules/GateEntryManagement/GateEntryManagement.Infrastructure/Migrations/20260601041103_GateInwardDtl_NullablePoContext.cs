using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardDtl_NullablePoContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DcQuantity",
                schema: "Gate",
                table: "GateInwardDtl",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PoId",
                schema: "Gate",
                table: "GateInwardDtl",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PoSlNoLocal",
                schema: "Gate",
                table: "GateInwardDtl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardDtl_PoId",
                schema: "Gate",
                table: "GateInwardDtl",
                column: "PoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GateInwardDtl_PoId",
                schema: "Gate",
                table: "GateInwardDtl");

            migrationBuilder.DropColumn(
                name: "DcQuantity",
                schema: "Gate",
                table: "GateInwardDtl");

            migrationBuilder.DropColumn(
                name: "PoId",
                schema: "Gate",
                table: "GateInwardDtl");

            migrationBuilder.DropColumn(
                name: "PoSlNoLocal",
                schema: "Gate",
                table: "GateInwardDtl");
        }
    }
}
