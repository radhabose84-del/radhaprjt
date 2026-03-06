using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotiLognew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendTo",
                schema: "AppNotification",
                table: "NotificationEventLog",
                type: "varchar(1000)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendTo",
                schema: "AppNotification",
                table: "NotificationEventLog");
        }
    }
}
