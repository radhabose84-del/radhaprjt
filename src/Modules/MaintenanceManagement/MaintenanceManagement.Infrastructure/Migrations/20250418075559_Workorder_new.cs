using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrder",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<short>(type: "smallint", nullable: false),
                    UnitId = table.Column<short>(type: "smallint", nullable: false),
                    WorkOrderDocNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: true),
                    PreventiveScheduleId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<short>(type: "smallint", nullable: false),
                    RootCauseId = table.Column<short>(type: "smallint", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Image = table.Column<string>(type: "varchar(250)", nullable: true),
                    TotalManPower = table.Column<short>(type: "smallint", nullable: true),
                    TotalSpentHours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
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
                    table.PrimaryKey("PK_WorkOrder", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrder",
                schema: "Maintenance");
        }
    }
}
