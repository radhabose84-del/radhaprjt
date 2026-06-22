using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CoaChangeRequestAndUnfreeze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastPostFreezeChangeOn",
                schema: "Finance",
                table: "GlAccountMaster",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoaUnfreezeRequest",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "varchar(1000)", nullable: false),
                    CfoApproverUserId = table.Column<int>(type: "int", nullable: true),
                    CfoApprovedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SysAdminApproverUserId = table.Column<int>(type: "int", nullable: true),
                    SysAdminApprovedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    WindowMinutes = table.Column<int>(type: "int", nullable: false),
                    WindowOpenedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WindowExpiry = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    RequestedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoaUnfreezeRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoaChangeRequest",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TargetAccountId = table.Column<int>(type: "int", nullable: true),
                    TargetAccountGroupId = table.Column<int>(type: "int", nullable: true),
                    AccountCodeSnapshot = table.Column<string>(type: "varchar(50)", nullable: true),
                    ChangeType = table.Column<string>(type: "varchar(50)", nullable: false),
                    Justification = table.Column<string>(type: "varchar(1000)", nullable: false),
                    ImpactAssessment = table.Column<string>(type: "varchar(2000)", nullable: false),
                    ImpactApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ImpactApprovedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UnfreezeRequestId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    IsPostFreeze = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CommittedByUserId = table.Column<int>(type: "int", nullable: true),
                    CommittedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    RequestedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoaChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoaChangeRequest_CoaUnfreezeRequest_UnfreezeRequestId",
                        column: x => x.UnfreezeRequestId,
                        principalSchema: "Finance",
                        principalTable: "CoaUnfreezeRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoaChangeRequest_CompanyId_Status",
                schema: "Finance",
                table: "CoaChangeRequest",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CoaChangeRequest_UnfreezeRequestId_Status",
                schema: "Finance",
                table: "CoaChangeRequest",
                columns: new[] { "UnfreezeRequestId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CoaUnfreezeRequest_CompanyId_Status",
                schema: "Finance",
                table: "CoaUnfreezeRequest",
                columns: new[] { "CompanyId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoaChangeRequest",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "CoaUnfreezeRequest",
                schema: "Finance");

            migrationBuilder.DropColumn(
                name: "LastPostFreezeChangeOn",
                schema: "Finance",
                table: "GlAccountMaster");
        }
    }
}
