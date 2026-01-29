using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MachineMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineMaster",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    MachineGroupId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ProductionCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    ShiftMasterId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: false),
                    InstallationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineMaster_CostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "Maintenance",
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineMaster_MachineGroup_MachineGroupId",
                        column: x => x.MachineGroupId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineMaster_ShiftMaster_ShiftMasterId",
                        column: x => x.ShiftMasterId,
                        principalSchema: "Maintenance",
                        principalTable: "ShiftMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineMaster_WorkCenter_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaster_CostCenterId",
                schema: "Maintenance",
                table: "MachineMaster",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaster_MachineGroupId",
                schema: "Maintenance",
                table: "MachineMaster",
                column: "MachineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaster_ShiftMasterId",
                schema: "Maintenance",
                table: "MachineMaster",
                column: "ShiftMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaster_WorkCenterId",
                schema: "Maintenance",
                table: "MachineMaster",
                column: "WorkCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineMaster",
                schema: "Maintenance");

         
        }
    }
}
