using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderActivity",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderActivity_ActivityMaster_ActivityId",
                        column: x => x.ActivityId,
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
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    CheckListId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderCheckList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderCheckList_ActivityCheckListMaster_CheckListId",
                        column: x => x.CheckListId,
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
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    OldItemId = table.Column<int>(type: "int", nullable: true),
                    ItemName = table.Column<string>(type: "varchar(250)", nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    AvailableQty = table.Column<int>(type: "int", nullable: false),
                    UsedQty = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "varchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderItem_MiscMaster_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderItem_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderSchedule",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    RepairStartTime = table.Column<DateTimeOffset>(type: "DateTimeOffset", nullable: false),
                    RepairEndTime = table.Column<DateTimeOffset>(type: "DateTimeOffset", nullable: false)
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
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: false),
                    OldTechnicianId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    TechnicianName = table.Column<string>(type: "varchar(100)", nullable: false),
                    HoursSpent = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderTechnician", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderTechnician_MiscMaster_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderTechnician_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderActivity_ActivityId",
                schema: "Maintenance",
                table: "WorkOrderActivity",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderActivity_WorkOrderId",
                schema: "Maintenance",
                table: "WorkOrderActivity",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCheckList_CheckListId",
                schema: "Maintenance",
                table: "WorkOrderCheckList",
                column: "CheckListId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCheckList_WorkOrderId",
                schema: "Maintenance",
                table: "WorkOrderCheckList",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_WorkOrderId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSchedule_WorkOrderId",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTechnician_SourceId",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTechnician_WorkOrderId",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderActivity",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrderCheckList",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrderItem",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrderSchedule",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrderTechnician",
                schema: "Maintenance");
        }
    }
}
