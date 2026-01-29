using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropActivityMachineGroupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

                    // Drop Foreign Key for ActivityMasterId
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMachineGroup_ActivityMaster_ActivityMasterId",
               
                table: "ActivityMachineGroup");

            // Drop Foreign Key for MachineGroupId
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMachineGroup_MachineGroup_MachineGroupId",
             
                table: "ActivityMachineGroup");

            // Drop the table
            migrationBuilder.DropTable(
                name: "ActivityMachineGroup"
               );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
