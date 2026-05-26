using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DispatchAdviceTransportMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransportMode",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_TransportMode",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "TransportMode");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchAdviceHeader_MiscMaster_TransportMode",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "TransportMode",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchAdviceHeader_MiscMaster_TransportMode",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAdviceHeader_TransportMode",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropColumn(
                name: "TransportMode",
                schema: "Sales",
                table: "DispatchAdviceHeader");
        }
    }
}
