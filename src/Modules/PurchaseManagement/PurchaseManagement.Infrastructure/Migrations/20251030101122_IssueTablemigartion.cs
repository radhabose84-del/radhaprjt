using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IssueTablemigartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "RequestQuantity",
                schema: "Purchase",
                table: "MrsDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0.000m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "IssueHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IssueNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    IssueDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    MrsHeaderId = table.Column<int>(type: "int", nullable: false),
                    SubStoresWarehouseId = table.Column<int>(type: "int", nullable: false),
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
                        principalSchema: "Purchase",
                        principalTable: "MrsHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    RequestQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    WarehouseStockId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    IssueQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    FinanceCode = table.Column<int>(type: "int", nullable: true),
                    SubStoresStorageTypeId = table.Column<int>(type: "int", nullable: false),
                    SubStoresTargetId = table.Column<int>(type: "int", nullable: false),
                    SubStoresPriorityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueDetail_IssueHeader_IssueHeaderId",
                        column: x => x.IssueHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "IssueHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueDetail_IssueHeaderId",
                schema: "Purchase",
                table: "IssueDetail",
                column: "IssueHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueHeader_MrsHeaderId",
                schema: "Purchase",
                table: "IssueHeader",
                column: "MrsHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "IssueHeader",
                schema: "Purchase");

            migrationBuilder.AlterColumn<decimal>(
                name: "RequestQuantity",
                schema: "Purchase",
                table: "MrsDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldDefaultValue: 0.000m);
        }
    }
}
