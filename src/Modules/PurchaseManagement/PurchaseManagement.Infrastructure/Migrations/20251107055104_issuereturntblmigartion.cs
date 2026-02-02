using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class issuereturntblmigartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueReturnHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IssueReturnNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    IssueReturnDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    IssueHeaderId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: false),
                    ApprovedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    ApprovedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ApprovedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueReturnHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueReturnHeader_IssueHeader_IssueHeaderId",
                        column: x => x.IssueHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "IssueHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueReturnHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueReturnDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueHeaderId = table.Column<int>(type: "int", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    WarehouseStockId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    ReturnValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: false),
                    ApprovedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: true),
                    ApprovedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ApprovedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueReturnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueReturnDetail_IssueReturnHeader_IssueHeaderId",
                        column: x => x.IssueHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "IssueReturnHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueReturnDetail_MiscMaster_ReasonId",
                        column: x => x.ReasonId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueReturnDetail_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueReturnDetail_IssueHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                column: "IssueHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReturnDetail_ReasonId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReturnDetail_StatusId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReturnHeader_IssueHeaderId",
                schema: "Purchase",
                table: "IssueReturnHeader",
                column: "IssueHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReturnHeader_StatusId",
                schema: "Purchase",
                table: "IssueReturnHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueReturnDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "IssueReturnHeader",
                schema: "Purchase");
        }
    }
}
