using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Activityissueactivityid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMachineGroup_ActivityMaster_ActivityMasterId",
                schema: "Maintenance",
                table: "ActivityMachineGroup");

            migrationBuilder.RenameColumn(
                name: "ActivityMasterId",
                schema: "Maintenance",
                table: "ActivityMachineGroup",
                newName: "ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityMachineGroup_ActivityMasterId",
                schema: "Maintenance",
                table: "ActivityMachineGroup",
                newName: "IX_ActivityMachineGroup_ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityMachineGroup_ActivityMaster_ActivityId",
                schema: "Maintenance",
                table: "ActivityMachineGroup",
                column: "ActivityId",
                principalSchema: "Maintenance",
                principalTable: "ActivityMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
