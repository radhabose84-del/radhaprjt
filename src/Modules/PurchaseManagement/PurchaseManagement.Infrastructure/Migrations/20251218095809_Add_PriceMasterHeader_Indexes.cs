using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_PriceMasterHeader_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidFrom",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidTo",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidFrom_IsActive",
                schema: "Purchase",
                table: "PriceMasterHeader",
                columns: new[] { "ItemId", "VendorId", "ValidFrom", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidTo_IsActive",
                schema: "Purchase",
                table: "PriceMasterHeader",
                columns: new[] { "ItemId", "VendorId", "ValidTo", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidFrom_IsActive",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidTo_IsActive",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidFrom",
                schema: "Purchase",
                table: "PriceMasterHeader",
                columns: new[] { "ItemId", "VendorId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidTo",
                schema: "Purchase",
                table: "PriceMasterHeader",
                columns: new[] { "ItemId", "VendorId", "ValidTo" });
        }
    }
}
