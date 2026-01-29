using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MachineMasterLineNoAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LineNo",
                schema: "Maintenance",
                table: "MachineMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaster_LineNo",
                schema: "Maintenance",
                table: "MachineMaster",
                column: "LineNo");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineMaster_MiscMaster_LineNo",
                schema: "Maintenance",
                table: "MachineMaster",
                column: "LineNo",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineMaster_MiscMaster_LineNo",
                schema: "Maintenance",
                table: "MachineMaster");

            migrationBuilder.DropIndex(
                name: "IX_MachineMaster_LineNo",
                schema: "Maintenance",
                table: "MachineMaster");

            migrationBuilder.DropColumn(
                name: "LineNo",
                schema: "Maintenance",
                table: "MachineMaster");
        }
    }
}
