using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class activitymastermachinegrprelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMaster_MachineGroup_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");

            migrationBuilder.DropIndex(
                name: "IX_ActivityMaster_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster");
        }
    }
}
