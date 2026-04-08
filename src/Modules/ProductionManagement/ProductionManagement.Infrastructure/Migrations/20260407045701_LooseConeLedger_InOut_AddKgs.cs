using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LooseConeLedger_InOut_AddKgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "RepackingHeader",
                newName: "LooseConeOut");

            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "LooseConeOut");

            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "LooseConeLedger",
                newName: "LooseConeOut");

            migrationBuilder.AddColumn<decimal>(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "RepackingHeader",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeIn",
                schema: "Production",
                table: "RepackingHeader",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeIn",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeIn",
                schema: "Production",
                table: "LooseConeLedger",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropColumn(
                name: "LooseConeIn",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropColumn(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "LooseConeIn",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "LooseConeIn",
                schema: "Production",
                table: "LooseConeLedger");

            migrationBuilder.RenameColumn(
                name: "LooseConeOut",
                schema: "Production",
                table: "RepackingHeader",
                newName: "LooseConeKgs");

            migrationBuilder.RenameColumn(
                name: "LooseConeOut",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "LooseConeKgs");

            migrationBuilder.RenameColumn(
                name: "LooseConeOut",
                schema: "Production",
                table: "LooseConeLedger",
                newName: "LooseConeKgs");
        }
    }
}
