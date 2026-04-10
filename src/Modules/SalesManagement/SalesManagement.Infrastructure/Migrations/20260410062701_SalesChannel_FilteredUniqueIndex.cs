using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesChannel_FilteredUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesChannel_SalesChannelCode",
                schema: "Sales",
                table: "SalesChannel");

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannel_SalesChannelCode",
                schema: "Sales",
                table: "SalesChannel",
                column: "SalesChannelCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesChannel_SalesChannelCode",
                schema: "Sales",
                table: "SalesChannel");

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannel_SalesChannelCode",
                schema: "Sales",
                table: "SalesChannel",
                column: "SalesChannelCode",
                unique: true);
        }
    }
}
