using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MRSTablechangesmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageTypeId",
                schema: "Purchase",
                table: "MrsDetail");

            migrationBuilder.DropColumn(
                name: "TargetId",
                schema: "Purchase",
                table: "MrsDetail");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                schema: "Purchase",
                table: "MrsDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StorageTypeId",
                schema: "Purchase",
                table: "MrsDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                schema: "Purchase",
                table: "MrsDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                schema: "Purchase",
                table: "MrsDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
