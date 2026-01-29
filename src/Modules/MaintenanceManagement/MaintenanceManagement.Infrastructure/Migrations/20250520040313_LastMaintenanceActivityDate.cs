using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LastMaintenanceActivityDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastMaintenanceActivityDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "LastMaintenanceActivityDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");
        }
    }
}
