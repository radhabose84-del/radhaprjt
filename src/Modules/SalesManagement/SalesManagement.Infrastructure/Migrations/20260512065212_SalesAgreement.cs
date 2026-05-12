using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesAgreementHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgreementNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SalesGroupId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_SalesAgreementHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesAgreementHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesAgreementHeader_SalesGroup_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalSchema: "Sales",
                        principalTable: "SalesGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesAgreementDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesAgreementHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    AgreedRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    TotalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ReleasedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesAgreementDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesAgreementDetail_SalesAgreementHeader_SalesAgreementHeaderId",
                        column: x => x.SalesAgreementHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesAgreementHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementDetail_ItemId",
                schema: "Sales",
                table: "SalesAgreementDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementDetail_SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesAgreementDetail",
                column: "SalesAgreementHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementDetail_SalesAgreementHeaderId_ItemId_VariantId",
                schema: "Sales",
                table: "SalesAgreementDetail",
                columns: new[] { "SalesAgreementHeaderId", "ItemId", "VariantId" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementDetail_VariantId",
                schema: "Sales",
                table: "SalesAgreementDetail",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementHeader_AgreementNo",
                schema: "Sales",
                table: "SalesAgreementHeader",
                column: "AgreementNo",
                unique: true,
                filter: "[AgreementNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementHeader_CustomerId",
                schema: "Sales",
                table: "SalesAgreementHeader",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementHeader_SalesGroupId",
                schema: "Sales",
                table: "SalesAgreementHeader",
                column: "SalesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementHeader_StatusId",
                schema: "Sales",
                table: "SalesAgreementHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementHeader_ValidFrom_ValidTo",
                schema: "Sales",
                table: "SalesAgreementHeader",
                columns: new[] { "ValidFrom", "ValidTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesAgreementDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesAgreementHeader",
                schema: "Sales");
        }
    }
}
