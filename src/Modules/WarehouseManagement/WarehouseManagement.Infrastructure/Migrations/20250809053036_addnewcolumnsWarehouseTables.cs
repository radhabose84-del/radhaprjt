using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addnewcolumnsWarehouseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaTypeId",
                schema: "Warehouse",
                table: "WarehouseMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsVirtualWarehouse",
                schema: "Warehouse",
                table: "WarehouseMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OperationTypeId",
                schema: "Warehouse",
                table: "WarehouseMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaTypeId",
                schema: "Warehouse",
                table: "WarehouseMaster");

            migrationBuilder.DropColumn(
                name: "IsVirtualWarehouse",
                schema: "Warehouse",
                table: "WarehouseMaster");

            migrationBuilder.DropColumn(
                name: "OperationTypeId",
                schema: "Warehouse",
                table: "WarehouseMaster");
        }
    }
}
