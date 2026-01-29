using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventLog_NotificationLevelHierarchy_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_MiscMaster_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy");

            migrationBuilder.RenameColumn(
                name: "NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "NotificationLevelHierarchyId");

            migrationBuilder.RenameColumn(
                name: "NotificationGroupMemberId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "NotificationChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationEventRule_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "IX_NotificationEventRule_NotificationLevelHierarchyId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "NotificationStatusId");

            migrationBuilder.RenameColumn(
                name: "NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "NotificationLevelRuleId");

            migrationBuilder.RenameColumn(
                name: "Action",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "ActionStatus");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationEventLog_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "IX_NotificationEventLog_NotificationLevelRuleId");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventRule_NotificationChannelId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEventLog_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventLog_MiscMaster_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationStatusId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventLog_NotificationEventRule_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationLevelRuleId",
                principalSchema: "AppNotification",
                principalTable: "NotificationEventRule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_MiscMaster_NotificationChannelId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationChannelId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationGroupId",
                principalSchema: "AppNotification",
                principalTable: "NotificationGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_NotificationLevelHierarchy_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationLevelHierarchyId",
                principalSchema: "AppNotification",
                principalTable: "NotificationLevelHierarchy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventLog_MiscMaster_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventLog_NotificationEventRule_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_MiscMaster_NotificationChannelId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventRule_NotificationLevelHierarchy_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropIndex(
                name: "IX_NotificationEventRule_NotificationChannelId",
                schema: "AppNotification",
                table: "NotificationEventRule");

            migrationBuilder.DropIndex(
                name: "IX_NotificationEventLog_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.RenameColumn(
                name: "NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "NotificationStatusId");

            migrationBuilder.RenameColumn(
                name: "NotificationChannelId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "NotificationGroupMemberId");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationEventRule_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                newName: "IX_NotificationEventRule_NotificationStatusId");

            migrationBuilder.RenameColumn(
                name: "NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "NotificationLevelHierarchyId");

            migrationBuilder.RenameColumn(
                name: "ActionStatus",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "Action");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationEventLog_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                newName: "IX_NotificationEventLog_NotificationLevelHierarchyId");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventLog_NotificationLevelHierarchy_NotificationLevelHierarchyId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationLevelHierarchyId",
                principalSchema: "AppNotification",
                principalTable: "NotificationLevelHierarchy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_MiscMaster_NotificationStatusId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationStatusId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventRule_NotificationGroup_NotificationGroupId",
                schema: "AppNotification",
                table: "NotificationEventRule",
                column: "NotificationGroupId",
                principalSchema: "AppNotification",
                principalTable: "NotificationGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
