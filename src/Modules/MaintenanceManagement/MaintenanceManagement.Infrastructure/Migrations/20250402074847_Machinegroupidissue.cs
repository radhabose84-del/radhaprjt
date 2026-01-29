using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Machinegroupidissue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMaster",
                type: "int",
                nullable: true);

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
                principalColumn: "Id");
        }
    }
}
