using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderAmendmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RevisionNumber",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SalesOrderAmendmentHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AmendmentNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    AmendmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_SalesOrderAmendmentHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderAmendmentHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderAmendmentHeader_SalesOrderHeader_SalesOrderHeaderId",
                        column: x => x.SalesOrderHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderAmendmentDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderAmendmentHeaderId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "varchar(10)", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    OldItemId = table.Column<int>(type: "int", nullable: false),
                    OldQtyInBags = table.Column<int>(type: "int", nullable: false),
                    OldExMillRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    OldExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NewQtyInBags = table.Column<int>(type: "int", nullable: true),
                    NewExMillRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    NewExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderAmendmentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderAmendmentDetail_SalesOrderAmendmentHeader_SalesOrderAmendmentHeaderId",
                        column: x => x.SalesOrderAmendmentHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderAmendmentHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderAmendmentDetail_SalesOrderDetail_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentDetail_SalesOrderAmendmentHeaderId",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                column: "SalesOrderAmendmentHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentDetail_SalesOrderDetailId",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentHeader_AmendmentNo",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                column: "AmendmentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentHeader_SalesOrderHeaderId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                column: "SalesOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentHeader_StatusId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentHeader_UnitId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderAmendmentDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderAmendmentHeader",
                schema: "Sales");

            migrationBuilder.DropColumn(
                name: "RevisionNumber",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
