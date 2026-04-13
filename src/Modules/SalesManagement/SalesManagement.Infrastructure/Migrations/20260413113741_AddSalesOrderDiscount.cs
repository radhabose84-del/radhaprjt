using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOrderDiscount",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    DiscountMasterId = table.Column<int>(type: "int", nullable: false),
                    SlabTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderDiscount_DiscountMaster_DiscountMasterId",
                        column: x => x.DiscountMasterId,
                        principalSchema: "Sales",
                        principalTable: "DiscountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderDiscount_MiscMaster_SlabTypeId",
                        column: x => x.SlabTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderDiscount_SalesOrderHeader_SalesOrderHeaderId",
                        column: x => x.SalesOrderHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDiscount_DiscountMasterId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                column: "DiscountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDiscount_SalesOrderHeaderId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                column: "SalesOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDiscount_SalesOrderHeaderId_DiscountMasterId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                columns: new[] { "SalesOrderHeaderId", "DiscountMasterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDiscount_SlabTypeId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                column: "SlabTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderDiscount",
                schema: "Sales");
        }
    }
}
