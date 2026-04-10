using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommissionSplitMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommissionSplit",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SplitCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    SplitName = table.Column<string>(type: "varchar(100)", nullable: false),
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
                    table.PrimaryKey("PK_CommissionSplit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommissionSplitDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommissionSplitId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ShareTypeId = table.Column<int>(type: "int", nullable: false),
                    ShareValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_CommissionSplitDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionSplitDetail_CommissionSplit_CommissionSplitId",
                        column: x => x.CommissionSplitId,
                        principalSchema: "Sales",
                        principalTable: "CommissionSplit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionSplitDetail_MiscMaster_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionSplitDetail_MiscMaster_ShareTypeId",
                        column: x => x.ShareTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionSplit_SplitCode",
                schema: "Sales",
                table: "CommissionSplit",
                column: "SplitCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionSplitDetail_CommissionSplitId",
                schema: "Sales",
                table: "CommissionSplitDetail",
                column: "CommissionSplitId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionSplitDetail_CommissionSplitId_RoleId",
                schema: "Sales",
                table: "CommissionSplitDetail",
                columns: new[] { "CommissionSplitId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionSplitDetail_RoleId",
                schema: "Sales",
                table: "CommissionSplitDetail",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionSplitDetail_ShareTypeId",
                schema: "Sales",
                table: "CommissionSplitDetail",
                column: "ShareTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionSplitDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "CommissionSplit",
                schema: "Sales");
        }
    }
}
