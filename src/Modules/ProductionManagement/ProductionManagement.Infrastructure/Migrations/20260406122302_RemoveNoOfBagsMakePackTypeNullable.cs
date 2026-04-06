using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNoOfBagsMakePackTypeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOfBags",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.AlterColumn<int>(
                name: "PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetWeightPerPack",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetWeightPerPack",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfBags",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
