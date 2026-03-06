using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BG_Log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventLog_NotificationEventRule_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.AlterColumn<string>(
                name: "SendTo",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(1000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MessageText",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar(Max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "ActionStatus",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(250)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar(250)");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventLog_NotificationEventRule_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationLevelRuleId",
                principalSchema: "AppNotification",
                principalTable: "NotificationEventRule",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationEventLog_NotificationEventRule_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.AlterColumn<string>(
                name: "SendTo",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(1000)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MessageText",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "Varchar(Max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionStatus",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "Varchar(250)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationEventLog_NotificationEventRule_NotificationLevelRuleId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                column: "NotificationLevelRuleId",
                principalSchema: "AppNotification",
                principalTable: "NotificationEventRule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
