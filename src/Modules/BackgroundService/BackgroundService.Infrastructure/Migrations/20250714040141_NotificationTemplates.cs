using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificationTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuleName",
                schema: "AppNotification",
                table: "NotificationTemplate");

            migrationBuilder.AddColumn<int>(
                name: "NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationTemplate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationTemplate",
                column: "NotificationConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplate_NotificationConfig_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationTemplate",
                column: "NotificationConfigId",
                principalSchema: "AppNotification",
                principalTable: "NotificationConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplate_NotificationConfig_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationTemplate");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplate_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationTemplate");

            migrationBuilder.DropColumn(
                name: "NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationTemplate");

            migrationBuilder.AddColumn<string>(
                name: "ModuleName",
                schema: "AppNotification",
                table: "NotificationTemplate",
                type: "Varchar(250)",
                nullable: false,
                defaultValue: "");
        }
    }
}
