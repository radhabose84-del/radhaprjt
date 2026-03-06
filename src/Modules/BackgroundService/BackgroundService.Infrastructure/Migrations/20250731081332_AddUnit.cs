using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationTemplate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationConfig",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationTemplate");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationGroup");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationEventLog");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationConfig");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
