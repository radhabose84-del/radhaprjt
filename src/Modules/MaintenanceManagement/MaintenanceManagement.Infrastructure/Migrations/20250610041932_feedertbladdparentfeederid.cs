using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class feedertbladdparentfeederid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentFeederId",
                schema: "Maintenance",
                table: "Feeder",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feeder_ParentFeederId",
                schema: "Maintenance",
                table: "Feeder",
                column: "ParentFeederId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feeder_Feeder_ParentFeederId",
                schema: "Maintenance",
                table: "Feeder",
                column: "ParentFeederId",
                principalSchema: "Maintenance",
                principalTable: "Feeder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feeder_Feeder_ParentFeederId",
                schema: "Maintenance",
                table: "Feeder");

            migrationBuilder.DropIndex(
                name: "IX_Feeder_ParentFeederId",
                schema: "Maintenance",
                table: "Feeder");

            migrationBuilder.DropColumn(
                name: "ParentFeederId",
                schema: "Maintenance",
                table: "Feeder");
        }
    }
}
