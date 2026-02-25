using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MRSTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MrsHeader",
                schema: "Purchase",
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
                    ApprovedByByName = table.Column<string>(type: "varchar(50)", nullable: true),
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
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrsHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubStoreStockLedger",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(50)", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSlNo = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ReceivedValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubStoreStockLedger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MrsDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MrsHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    RequestQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    CancelledQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    FinanceCode = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrsDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MrsDetail_MrsHeader_MrsHeaderId",
                        column: x => x.MrsHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "MrsHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MrsDetail_MrsHeaderId",
                schema: "Purchase",
                table: "MrsDetail",
                column: "MrsHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_MrsHeader_RequestCategoryId",
                schema: "Purchase",
                table: "MrsHeader",
                column: "RequestCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MrsHeader_StatusId",
                schema: "Purchase",
                table: "MrsHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MrsDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "SubStoreStockLedger",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "MrsHeader",
                schema: "Purchase");
        }
    }
}
