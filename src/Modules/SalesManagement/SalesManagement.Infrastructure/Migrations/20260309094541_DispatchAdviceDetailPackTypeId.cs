using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DispatchAdviceDetailPackTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackTypeId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceDetail_PackTypeId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                column: "PackTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchAdviceDetail_PackType_PackTypeId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                column: "PackTypeId",
                principalSchema: "Sales",
                principalTable: "PackType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchAdviceDetail_PackType_PackTypeId",
                schema: "Sales",
                table: "DispatchAdviceDetail");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAdviceDetail_PackTypeId",
                schema: "Sales",
                table: "DispatchAdviceDetail");

            migrationBuilder.DropColumn(
                name: "PackTypeId",
                schema: "Sales",
                table: "DispatchAdviceDetail");
        }
    }
}
