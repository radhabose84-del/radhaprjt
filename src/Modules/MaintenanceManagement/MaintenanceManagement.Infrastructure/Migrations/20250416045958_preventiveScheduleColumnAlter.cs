using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveScheduleColumnAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHdr_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHdr_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerDtl",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerHdr",
                schema: "Maintenance");

            migrationBuilder.AlterColumn<string>(
                name: "OldItemId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                type: "varchar(250)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)");

            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerHeader",
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
                    table.PrimaryKey("PK_PreventiveSchedulerHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHeader_MachineGroup_MachineGroupId",
                        column: x => x.MachineGroupId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHeader_MaintenanceCategory_MaintenanceCategoryId",
                        column: x => x.MaintenanceCategoryId,
                        principalSchema: "Maintenance",
                        principalTable: "MaintenanceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHeader_MiscMaster_DueTypeId",
                        column: x => x.DueTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerHeader_MiscMaster_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerDetail",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreventiveSchedulerId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RescheduleReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreventiveSchedulerDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerDetail_MachineMaster_MachineId",
                        column: x => x.MachineId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreventiveSchedulerDetail_PreventiveSchedulerHeader_PreventiveSchedulerId",
                        column: x => x.PreventiveSchedulerId,
                        principalSchema: "Maintenance",
                        principalTable: "PreventiveSchedulerHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDetail_MachineId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDetail_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "PreventiveSchedulerId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHeader_DueTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "DueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHeader_FrequencyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHeader_MachineGroupId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "MachineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHeader_MaintenanceCategoryId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "MaintenanceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerHeader_ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHeader_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "PreventiveSchedulerHdrId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHeader_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                column: "PreventiveSchedulerId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHeader_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHeader_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerDetail",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "PreventiveSchedulerHeader",
                schema: "Maintenance");

            migrationBuilder.DropColumn(
                name: "SourceId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.AlterColumn<string>(
                name: "OldItemId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PreventiveSchedulerHdr",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DueTypeId = table.Column<int>(type: "int", nullable: false),
                    FrequencyId = table.Column<int>(type: "int", nullable: false),
                    MachineGroupId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceCategoryId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DownTimeEstimateHrs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DuePeriod = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GraceDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDownTimeRequired = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    ReminderMaterialReqDays = table.Column<int>(type: "int", nullable: false),
                    ReminderWorkOrderDays = table.Column<int>(type: "int", nullable: false)
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
                name: "PreventiveSchedulerDtl",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PreventiveSchedulerId = table.Column<int>(type: "int", nullable: false),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHdr_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "PreventiveSchedulerHdrId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHdr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHdr_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                column: "PreventiveSchedulerId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHdr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
