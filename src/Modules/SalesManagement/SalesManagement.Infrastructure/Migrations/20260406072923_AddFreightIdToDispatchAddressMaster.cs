using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFreightIdToDispatchAddressMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreightId",
                schema: "Sales",
                table: "DispatchAddressMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMaster_FreightId",
                schema: "Sales",
                table: "DispatchAddressMaster",
                column: "FreightId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchAddressMaster_FreightMaster_FreightId",
                schema: "Sales",
                table: "DispatchAddressMaster",
                column: "FreightId",
                principalSchema: "Sales",
                principalTable: "FreightMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchAddressMaster_FreightMaster_FreightId",
                schema: "Sales",
                table: "DispatchAddressMaster");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAddressMaster_FreightId",
                schema: "Sales",
                table: "DispatchAddressMaster");

            migrationBuilder.DropColumn(
                name: "FreightId",
                schema: "Sales",
                table: "DispatchAddressMaster");
        }
    }
}
