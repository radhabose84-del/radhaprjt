using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceRequest_ConvertedToPoAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedToPoAmount",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedToPoAmount",
                schema: "Maintenance",
                table: "MaintenanceRequest");
        }
    }
}
