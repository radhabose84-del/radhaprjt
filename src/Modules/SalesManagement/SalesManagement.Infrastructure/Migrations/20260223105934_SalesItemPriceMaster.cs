using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesItemPriceMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesItemPriceMaster",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SalesSegmentId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    ExMillPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_SalesItemPriceMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesItemPriceMaster_SalesSegment_SalesSegmentId",
                        column: x => x.SalesSegmentId,
                        principalSchema: "Sales",
                        principalTable: "SalesSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemPriceMaster_ItemId_SalesSegmentId_PaymentTermsId",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                columns: new[] { "ItemId", "SalesSegmentId", "PaymentTermsId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemPriceMaster_PriceCode",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                column: "PriceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesItemPriceMaster_SalesSegmentId",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                column: "SalesSegmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesItemPriceMaster",
                schema: "Sales");
        }
    }
}
