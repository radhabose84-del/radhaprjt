using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransportDetail_AddModuleId_DefaultProcurementRateBasisId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultProcurementRateBasisId",
                schema: "Party",
                table: "TransportDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                schema: "Party",
                table: "TransportDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_DefaultProcurementRateBasisId",
                schema: "Party",
                table: "TransportDetail",
                column: "DefaultProcurementRateBasisId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_ModuleId",
                schema: "Party",
                table: "TransportDetail",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportDetail_MiscMaster_DefaultProcurementRateBasisId",
                schema: "Party",
                table: "TransportDetail",
                column: "DefaultProcurementRateBasisId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportDetail_MiscMaster_DefaultProcurementRateBasisId",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropIndex(
                name: "IX_TransportDetail_DefaultProcurementRateBasisId",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropIndex(
                name: "IX_TransportDetail_ModuleId",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "DefaultProcurementRateBasisId",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "Party",
                table: "TransportDetail");
        }
    }
}
