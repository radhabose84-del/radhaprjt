using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotiLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventLog_ReadStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "ReadStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventLog_MiscMaster_ReadStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "ReadStatusId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventLog_MiscMaster_ReadStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.DropIndex(
                name: "IX_NotificationEventLog_ReadStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.DropColumn(
                name: "ReadStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog");
        }
    }
}
