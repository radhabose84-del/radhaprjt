using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LooseConeLedger_RemoveInOut_AddKgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LooseConeIn",
                schema: "Production",
                table: "LooseConeLedger");

            migrationBuilder.RenameColumn(
                name: "LooseConeOut",
                schema: "Production",
                table: "LooseConeLedger",
                newName: "LooseConeKgs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "LooseConeLedger",
                newName: "LooseConeOut");

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeIn",
                schema: "Production",
                table: "LooseConeLedger",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
