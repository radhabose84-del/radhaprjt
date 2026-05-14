using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceLine_RequestId_CheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrderServiceLine_RequestId_NotZero",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine",
                sql: "[RequestId] IS NULL OR [RequestId] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrderServiceLine_RequestId_NotZero",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine");
        }
    }
}
