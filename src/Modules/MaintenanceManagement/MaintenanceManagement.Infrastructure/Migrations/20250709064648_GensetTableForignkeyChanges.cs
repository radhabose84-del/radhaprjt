using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GensetTableForignkeyChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratorConsumption_Generator_GeneratorId",
                schema: "Maintenance",
                table: "GeneratorConsumption");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratorConsumption_MachineMaster_GeneratorId",
                schema: "Maintenance",
                table: "GeneratorConsumption",
                column: "GeneratorId",
                principalSchema: "Maintenance",
                principalTable: "MachineMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratorConsumption_MachineMaster_GeneratorId",
                schema: "Maintenance",
                table: "GeneratorConsumption");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratorConsumption_Generator_GeneratorId",
                schema: "Maintenance",
                table: "GeneratorConsumption",
                column: "GeneratorId",
                principalSchema: "Maintenance",
                principalTable: "Generator",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
