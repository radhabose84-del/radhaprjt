using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Activityperelationshipwithmisc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ActivityMaster_ActivityType",
                schema: "Maintenance",
                table: "ActivityMaster",
                column: "ActivityType");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityMaster_MiscMaster_ActivityType",
                schema: "Maintenance",
                table: "ActivityMaster",
                column: "ActivityType",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityMaster_MiscMaster_ActivityType",
                schema: "Maintenance",
                table: "ActivityMaster");

            migrationBuilder.DropIndex(
                name: "IX_ActivityMaster_ActivityType",
                schema: "Maintenance",
                table: "ActivityMaster");
        }
    }
}
