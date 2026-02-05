using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MRSandIssuetblmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MrsHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    RequestCategoryId = table.Column<int>(type: "int", nullable: false),
                    MrsNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    MrsDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SubDepartmentId = table.Column<int>(type: "int", nullable: false),
                    SubStoresWarehouseId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    ApprovedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ApprovedIP = table.Column<string>(type: "varchar(20)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrsHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MrsHeader_MiscMaster_RequestCategoryId",
                        column: x => x.RequestCategoryId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrsHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IssueNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    IssueDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    MrsHeaderId = table.Column<int>(type: "int", nullable: false),
                    SubStoresWarehouseId = table.Column<int>(type: "int", nullable: true),
                    IssuedBy = table.Column<int>(type: "int", nullable: false),
                    IssuedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IssuedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    IssuedIp = table.Column<string>(type: "varchar(20)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueHeader_MrsHeader_MrsHeaderId",
                        column: x => x.MrsHeaderId,
                        principalSchema: "Inventory",
                        principalTable: "MrsHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MrsDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MrsHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    RequestQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    FinanceCode = table.Column<int>(type: "int", nullable: true),
                    WarehouseStockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrsDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MrsDetail_MrsHeader_MrsHeaderId",
                        column: x => x.MrsHeaderId,
                        principalSchema: "Inventory",
                        principalTable: "MrsHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueHeaderId = table.Column<int>(type: "int", nullable: false),
                    Sno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    RequestQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    WarehouseStockId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    IssueQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    FinanceCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueDetail_IssueHeader_IssueHeaderId",
                        column: x => x.IssueHeaderId,
                        principalSchema: "Inventory",
                        principalTable: "IssueHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueDetail_IssueHeaderId",
                schema: "Inventory",
                table: "IssueDetail",
                column: "IssueHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueHeader_MrsHeaderId",
                schema: "Inventory",
                table: "IssueHeader",
                column: "MrsHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_MrsDetail_MrsHeaderId",
                schema: "Inventory",
                table: "MrsDetail",
                column: "MrsHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_MrsHeader_RequestCategoryId",
                schema: "Inventory",
                table: "MrsHeader",
                column: "RequestCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MrsHeader_StatusId",
                schema: "Inventory",
                table: "MrsHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MrsDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "IssueHeader",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MrsHeader",
                schema: "Inventory");
        }
    }
}
