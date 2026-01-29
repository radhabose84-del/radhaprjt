using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class drop_workorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderActivity",
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

            migrationBuilder.DropTable(
                name: "WorkOrder",
                schema: "Maintenance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrder",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    RequestTypeId = table.Column<int>(type: "int", nullable: false),
                    RootCauseId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderTypeId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    Image = table.Column<string>(type: "varchar(250)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastActivityDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    OldVendorId = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(1000)", nullable: true),
                    RequestId = table.Column<string>(type: "varchar(25)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrder_MaintenanceCategory_WorkOrderTypeId",
                        column: x => x.WorkOrderTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MaintenanceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_MiscMaster_PriorityId",
                        column: x => x.PriorityId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_MiscMaster_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_MiscMaster_RootCauseId",
                        column: x => x.RootCauseId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrder_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderActivity",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false),
                    EstimatedTime = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
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
                name: "WorkOrderItem",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    AvailableQty = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "varchar(250)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    UsedQty = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderItem", x => x.Id);
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
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    DownTimeEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DownTimeStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    RepairEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    RepairStartTime = table.Column<TimeSpan>(type: "time", nullable: false)
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
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    HoursSpent = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    TechnicianId = table.Column<int>(type: "int", nullable: false),
                    TechnicianName = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderTechnician", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderTechnician_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_PriorityId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_RequestTypeId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_RootCauseId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "RootCauseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_StatusId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_WorkOrderTypeId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "WorkOrderTypeId");

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
                name: "IX_WorkOrderTechnician_WorkOrderId",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                column: "WorkOrderId");
        }
    }
}
