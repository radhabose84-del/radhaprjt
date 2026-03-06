using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificationEventRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_MiscMaster_NotificationTypeId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_NotificationConfig_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropIndex(
                name: "IX_NotificationEventRule_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropColumn(
                name: "NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.RenameColumn(
                name: "NotificationTypeId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationEventRule_NotificationTypeId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "IX_NotificationEventRule_TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_NotificationTemplate_TemplateId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "TemplateId",
                principalSchema: "AppNotification",
                principalTable: "NotificationTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_NotificationTemplate_TemplateId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "NotificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationEventRule_TemplateId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "IX_NotificationEventRule_NotificationTypeId");

            migrationBuilder.AddColumn<int>(
                name: "NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_MiscMaster_NotificationTypeId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_NotificationConfig_NotificationConfigId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationConfigId",
                principalSchema: "AppNotification",
                principalTable: "NotificationConfig",
                principalColumn: "Id");
        }
    }
}
