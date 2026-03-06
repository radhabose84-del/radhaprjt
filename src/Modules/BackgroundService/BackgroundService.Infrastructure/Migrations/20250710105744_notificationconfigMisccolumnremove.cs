using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class notificationconfigMisccolumnremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationConfig_MiscMaster_MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig");

            migrationBuilder.DropIndex(
                name: "IX_NotificationConfig_MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationConfig_MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationConfig_MiscMaster_MiscMasterId",
                schema: "AppNotification",
                table: "NotificationConfig",
                column: "MiscMasterId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
