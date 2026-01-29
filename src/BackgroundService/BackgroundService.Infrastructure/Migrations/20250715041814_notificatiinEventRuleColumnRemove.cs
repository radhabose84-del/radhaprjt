using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class notificatiinEventRuleColumnRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropIndex(
                name: "IX_NotificationEventRule_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropColumn(
                name: "NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationGroupId",
                principalSchema: "AppNotification",
                principalTable: "NotificationGroup",
                principalColumn: "Id");
        }
    }
}
