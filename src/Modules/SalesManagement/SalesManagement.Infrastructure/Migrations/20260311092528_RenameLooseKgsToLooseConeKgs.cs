using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameLooseKgsToLooseConeKgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LooseKgs",
                schema: "Production",
                table: "ProductionPackHeader",
                newName: "LooseConeKgs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackHeader",
                newName: "LooseKgs");
        }
    }
}
