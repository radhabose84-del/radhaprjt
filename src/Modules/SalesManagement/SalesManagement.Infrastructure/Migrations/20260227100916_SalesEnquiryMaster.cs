using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesEnquiryMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesEnquiryHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    EnquiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ContactPerson = table.Column<string>(type: "varchar(200)", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesEnquiryHeader", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesEnquiryDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesEnquiryHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ExmillRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    TargetPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesEnquiryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesEnquiryDetail_SalesEnquiryHeader_SalesEnquiryHeaderId",
                        column: x => x.SalesEnquiryHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesEnquiryHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryDetail_ItemId",
                schema: "Sales",
                table: "SalesEnquiryDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryDetail_SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesEnquiryDetail",
                column: "SalesEnquiryHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryHeader_PartyId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesEnquiryDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesEnquiryHeader",
                schema: "Sales");
        }
    }
}
