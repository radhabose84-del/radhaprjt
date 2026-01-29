using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migrationtest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMaster_MachineGroup_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");

            migrationBuilder.DropTable(
                name: "MachineMaster",
                schema: "Maintenance");

            migrationBuilder.DropIndex(
                name: "IX_ActivityMaster_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");

            migrationBuilder.DropColumn(
                name: "MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");
        }
    }
}
