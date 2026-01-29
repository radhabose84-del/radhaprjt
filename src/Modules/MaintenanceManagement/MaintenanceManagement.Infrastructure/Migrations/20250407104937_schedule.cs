using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerHdr",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineGroupId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceCategoryId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    DueTypeId = table.Column<int>(type: "int", nullable: false),
                    DuePeriod = table.Column<int>(type: "int", nullable: false),
                    FrequencyId = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GraceDays = table.Column<int>(type: "int", nullable: false),
                    ReminderWorkOrderDays = table.Column<int>(type: "int", nullable: false),
                    ReminderMaterialReqDays = table.Column<int>(type: "int", nullable: false),
                    IsDownTimeRequired = table.Column<bool>(type: "bit", nullable: false),
                    DownTimeEstimateHrs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveSchedulerHdr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHdr_MachineGroup_MachineGroupId",
                        column: x => x.MachineGroupId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHdr_MaintenanceCategory_MaintenanceCategoryId",
                        column: x => x.MaintenanceCategoryId,
                        principalSchema: "Maintenance",
                        principalTable: "MaintenanceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHdr_MiscMaster_DueTypeId",
                        column: x => x.DueTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHdr_MiscMaster_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHdr_MiscMaster_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerActivity",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreventiveSchedulerHdrId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    EstimatedTimeHrs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveSchedulerActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerActivity_ActivityMaster_ActivityId",
                        column: x => x.ActivityId,
                        principalSchema: "Maintenance",
                        principalTable: "ActivityMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHdr_PreventiveSchedulerHdrId",
                        column: x => x.PreventiveSchedulerHdrId,
                        principalSchema: "Maintenance",
                        principalTable: "PreventiveSchedulerHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerDtl",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreventiveSchedulerId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveSchedulerDtl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerDtl_MachineMaster_MachineId",
                        column: x => x.MachineId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerDtl_PreventiveSchedulerHdr_PreventiveSchedulerId",
                        column: x => x.PreventiveSchedulerId,
                        principalSchema: "Maintenance",
                        principalTable: "PreventiveSchedulerHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerItems",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreventiveSchedulerId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RequiredQty = table.Column<int>(type: "int", nullable: false),
                    OldItemId = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveSchedulerItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHdr_PreventiveSchedulerId",
                        column: x => x.PreventiveSchedulerId,
                        principalSchema: "Maintenance",
                        principalTable: "PreventiveSchedulerHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerActivity_ActivityId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerActivity_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "PreventiveSchedulerHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDtl_MachineId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDtl",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDtl_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDtl",
                column: "PreventiveSchedulerId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHdr_DueTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHdr",
                column: "DueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHdr_FrequencyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHdr",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHdr_MachineGroupId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHdr",
                column: "MachineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHdr_MaintenanceCategoryId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHdr",
                column: "MaintenanceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHdr_ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHdr",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerItems_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                column: "PreventiveSchedulerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreventiveSchedulerActivity",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerDtl",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerItems",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerHdr",
                schema: "Maintenance");
        }
    }
}
