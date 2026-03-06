using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeaderStatusIdToStoHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeaderStatusId",
                schema: "Sales",
                table: "StoHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_HeaderStatusId",
                schema: "Sales",
                table: "StoHeader",
                column: "HeaderStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoHeader_MiscMaster_HeaderStatusId",
                schema: "Sales",
                table: "StoHeader",
                column: "HeaderStatusId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoHeader_MiscMaster_HeaderStatusId",
                schema: "Sales",
                table: "StoHeader");

            migrationBuilder.DropIndex(
                name: "IX_StoHeader_HeaderStatusId",
                schema: "Sales",
                table: "StoHeader");

            migrationBuilder.DropColumn(
                name: "HeaderStatusId",
                schema: "Sales",
                table: "StoHeader");
        }
    }
}
