using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class issuetblremovefield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubStoresPriorityId",
                schema: "Purchase",
                table: "IssueDetail");

            migrationBuilder.DropColumn(
                name: "SubStoresStorageTypeId",
                schema: "Purchase",
                table: "IssueDetail");

            migrationBuilder.DropColumn(
                name: "SubStoresTargetId",
                schema: "Purchase",
                table: "IssueDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubStoresPriorityId",
                schema: "Purchase",
                table: "IssueDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubStoresStorageTypeId",
                schema: "Purchase",
                table: "IssueDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubStoresTargetId",
                schema: "Purchase",
                table: "IssueDetail",
                type: "int",
                nullable: true);
        }
    }
}
