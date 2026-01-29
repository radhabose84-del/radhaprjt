using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_new_check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderActivity");

            migrationBuilder.DropTable(
                name: "WorkOrderCheckList");

            migrationBuilder.DropTable(
                name: "WorkOrderItem");

            migrationBuilder.DropTable(
                name: "WorkOrderSchedule");

            migrationBuilder.DropTable(
                name: "WorkOrderTechnician");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderActivity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityMasterId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderActivity_ActivityMaster_ActivityMasterId",
                        column: x => x.ActivityMasterId,
                        principalSchema: "Maintenance",
                        principalTable: "ActivityMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderActivity_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderCheckList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckListMasterId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    CheckListId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderCheckList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderCheckList_ActivityCheckListMaster_CheckListMasterId",
                        column: x => x.CheckListMasterId,
                        principalSchema: "Maintenance",
                        principalTable: "ActivityCheckListMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderCheckList_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MiscSourceId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    AvailableQty = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldItemId = table.Column<int>(type: "int", nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    UsedQty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderItem_MiscMaster_MiscSourceId",
                        column: x => x.MiscSourceId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderItem_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    RepairEndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RepairStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderSchedule_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderTechnician",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MiscSourceId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    HoursSpent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldTechnicianId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: false),
                    TechnicianName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderTechnician", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderTechnician_MiscMaster_MiscSourceId",
                        column: x => x.MiscSourceId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderTechnician_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderActivity_ActivityMasterId",
                table: "WorkOrderActivity",
                column: "ActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderActivity_WorkOrderId",
                table: "WorkOrderActivity",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCheckList_CheckListMasterId",
                table: "WorkOrderCheckList",
                column: "CheckListMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCheckList_WorkOrderId",
                table: "WorkOrderCheckList",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_MiscSourceId",
                table: "WorkOrderItem",
                column: "MiscSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_WorkOrderId",
                table: "WorkOrderItem",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSchedule_WorkOrderId",
                table: "WorkOrderSchedule",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTechnician_MiscSourceId",
                table: "WorkOrderTechnician",
                column: "MiscSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTechnician_WorkOrderId",
                table: "WorkOrderTechnician",
                column: "WorkOrderId");
        }
    }
}
