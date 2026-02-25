using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GRNtblmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PoMethodId",
                schema: "Purchase",
                table: "GateEntryDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GrnHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    GrnNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    GrnDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    GateEntryId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    DcNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    DcDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    ReceivingWarehouseId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    IsGrnGenerated = table.Column<bool>(type: "bit", nullable: false),
                    GrnReceivedImage = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true),
                    QcRemarks = table.Column<string>(type: "varchar(250)", nullable: true),
                    QcPersonName = table.Column<string>(type: "varchar(50)", nullable: true),
                    QcStatusId = table.Column<int>(type: "int", nullable: true),
                    QcDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    QcApprovedIp = table.Column<string>(type: "varchar(20)", nullable: true),
                    IsQcApproved = table.Column<bool>(type: "bit", nullable: false),
                    QcWarehouseId = table.Column<int>(type: "int", nullable: true),
                    RejectedImage = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    IsPutAwayRuleApproved = table.Column<bool>(type: "bit", nullable: false),
                    PutAwayRuleApprovedByName = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    PutAwayRuleApprovedIp = table.Column<string>(type: "varchar(20)", nullable: true),
                    PutAwayRuleApprovedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrnHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrnHeader_GateEntryHeader_GateEntryId",
                        column: x => x.GateEntryId,
                        principalSchema: "Purchase",
                        principalTable: "GateEntryHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrnHeader_MiscMaster_QcStatusId",
                        column: x => x.QcStatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrnDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnId = table.Column<int>(type: "int", nullable: false),
                    GrnSlNo = table.Column<int>(type: "int", nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    PoSlNoLocal = table.Column<int>(type: "int", nullable: true),
                    PoCategoryId = table.Column<int>(type: "int", nullable: false),
                    PoMethodId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OrderQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    DcQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    UpperTolerance = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    LowerTolerance = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    QcAcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    QcRejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    QcRejectedRemarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    BinId = table.Column<int>(type: "int", nullable: true),
                    RackId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrnDetail_GrnHeader_GrnId",
                        column: x => x.GrnId,
                        principalSchema: "Purchase",
                        principalTable: "GrnHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrnDetail_MiscMaster_PoCategoryId",
                        column: x => x.PoCategoryId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrnDetail_MiscMaster_PoMethodId",
                        column: x => x.PoMethodId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrnDetail_PurchaseOrderHeader_PoId",
                        column: x => x.PoId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryDetail_PoId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryDetail_PoMethodId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnDetail_GrnId",
                schema: "Purchase",
                table: "GrnDetail",
                column: "GrnId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnDetail_PoCategoryId",
                schema: "Purchase",
                table: "GrnDetail",
                column: "PoCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnDetail_PoId",
                schema: "Purchase",
                table: "GrnDetail",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnDetail_PoMethodId",
                schema: "Purchase",
                table: "GrnDetail",
                column: "PoMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnHeader_GateEntryId",
                schema: "Purchase",
                table: "GrnHeader",
                column: "GateEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_GrnHeader_QcStatusId",
                schema: "Purchase",
                table: "GrnHeader",
                column: "QcStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoMethodId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoMethodId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryDetail_PurchaseOrderHeader_PoId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoMethodId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryDetail_PurchaseOrderHeader_PoId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropTable(
                name: "GrnDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "GrnHeader",
                schema: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_GateEntryDetail_PoId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropIndex(
                name: "IX_GateEntryDetail_PoMethodId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropColumn(
                name: "PoMethodId",
                schema: "Purchase",
                table: "GateEntryDetail");
        }
    }
}
