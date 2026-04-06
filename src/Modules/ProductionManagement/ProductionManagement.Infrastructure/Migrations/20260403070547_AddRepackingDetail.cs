using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepackingDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepackingDetail_OldBinId",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropIndex(
                name: "IX_RepackingDetail_OldWarehouseId",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldBinId",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldLotId",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldNetWeight",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldNetWeightPerPack",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldTotalBags",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldWarehouseId",
                schema: "Production",
                table: "RepackingDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OldBinId",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OldLotId",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OldNetWeight",
                schema: "Production",
                table: "RepackingDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OldNetWeightPerPack",
                schema: "Production",
                table: "RepackingDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OldTotalBags",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OldWarehouseId",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_OldBinId",
                schema: "Production",
                table: "RepackingDetail",
                column: "OldBinId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_OldWarehouseId",
                schema: "Production",
                table: "RepackingDetail",
                column: "OldWarehouseId");
        }
    }
}
