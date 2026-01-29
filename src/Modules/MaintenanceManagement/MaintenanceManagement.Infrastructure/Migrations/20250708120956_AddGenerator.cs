using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Generator_MiscMaster_GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.DropIndex(
                name: "IX_Generator_GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.DropColumn(
                name: "GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.RenameColumn(
                name: "GensetStatus",
                schema: "Maintenance",
                table: "Generator",
                newName: "GensetStatusId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Voltage",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PowerFactor",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Power",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Frequency",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Current",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "ClosingEnergyReading",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningEnergyReading",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Generator_GensetStatusId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Generator_MiscMaster_GensetStatusId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Generator_MiscMaster_GensetStatusId",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.DropIndex(
                name: "IX_Generator_GensetStatusId",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.DropColumn(
                name: "ClosingEnergyReading",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.DropColumn(
                name: "OpeningEnergyReading",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.RenameColumn(
                name: "GensetStatusId",
                schema: "Maintenance",
                table: "Generator",
                newName: "GensetStatus");

            migrationBuilder.AlterColumn<decimal>(
                name: "Voltage",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PowerFactor",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Power",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Frequency",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Current",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AddColumn<int>(
                name: "GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Generator_GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Generator_MiscMaster_GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
