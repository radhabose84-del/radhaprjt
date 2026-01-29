using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removegroupid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMaster_MachineGroup_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");

            migrationBuilder.DropIndex(
                name: "IX_ActivityMaster_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");     

                // Drop the MachineGroupId column
                migrationBuilder.DropColumn(
                    name: "MachineGroupId",
                    schema: "Maintenance",
                    table: "ActivityMaster");       

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           // Recreate the MachineGroupId column
            migrationBuilder.AddColumn<int>(
                name: "MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster",
                type: "int",
                nullable: false,
                defaultValue: 0); // Change defaultValue if needed


            migrationBuilder.CreateIndex(
                name: "IX_ActivityMaster_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster",
                column: "MachineGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityMaster_MachineGroup_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster",
                column: "MachineGroupId",
                principalSchema: "Maintenance",
                principalTable: "MachineGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
