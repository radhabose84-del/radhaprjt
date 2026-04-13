using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LotMaster_AddVariantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                schema: "Production",
                table: "LotMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotMaster_VariantId",
                schema: "Production",
                table: "LotMaster",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LotMaster_VariantId",
                schema: "Production",
                table: "LotMaster");

            migrationBuilder.DropColumn(
                name: "VariantId",
                schema: "Production",
                table: "LotMaster");
        }
    }
}
