using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActivityMachineGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                // Recreate ActivityMachineGroup table in the "Maintenance" schema
            migrationBuilder.CreateTable(
                name: "ActivityMachineGroup",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityMasterId = table.Column<int>(type: "int", nullable: false),
                    MachineGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityMachineGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityMachineGroup_ActivityMaster_ActivityMasterId",
                        column: x => x.ActivityMasterId,
                        principalSchema: "Maintenance",
                        principalTable: "ActivityMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityMachineGroup_MachineGroup_MachineGroupId",
                        column: x => x.MachineGroupId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityMachineGroup_ActivityMasterId",
                schema: "Maintenance",
                table: "ActivityMachineGroup",
                column: "ActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityMachineGroup_MachineGroupId",
                schema: "Maintenance",
                table: "ActivityMachineGroup",
                column: "MachineGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityMachineGroup",
                schema: "Maintenance"
            );
        }
    }
}
