using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CustomerVisitMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerVisit",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VisitTypeId = table.Column<int>(type: "int", nullable: false),
                    VisitDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 18, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 18, scale: 6, nullable: true),
                    ImageName = table.Column<string>(type: "varchar(500)", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    MarketingOfficerId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CustomerVisit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerVisit_MarketingOfficer_MarketingOfficerId",
                        column: x => x.MarketingOfficerId,
                        principalSchema: "Sales",
                        principalTable: "MarketingOfficer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerVisit_MiscMaster_VisitTypeId",
                        column: x => x.VisitTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVisitProduct",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerVisitId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerVisitProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerVisitProduct_CustomerVisit_CustomerVisitId",
                        column: x => x.CustomerVisitId,
                        principalSchema: "Sales",
                        principalTable: "CustomerVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisit_CustomerId",
                schema: "Sales",
                table: "CustomerVisit",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisit_MarketingOfficerId",
                schema: "Sales",
                table: "CustomerVisit",
                column: "MarketingOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisit_VisitDateTime",
                schema: "Sales",
                table: "CustomerVisit",
                column: "VisitDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisit_VisitTypeId",
                schema: "Sales",
                table: "CustomerVisit",
                column: "VisitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisitProduct_CustomerVisitId",
                schema: "Sales",
                table: "CustomerVisitProduct",
                column: "CustomerVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisitProduct_ItemId",
                schema: "Sales",
                table: "CustomerVisitProduct",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerVisitProduct",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "CustomerVisit",
                schema: "Sales");
        }
    }
}
