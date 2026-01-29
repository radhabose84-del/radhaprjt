using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Alter_NotificationWhatsAppGroup_ApiKey_ToVarcharMax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                schema: "AppNotification",
                table: "NotificationWhatsAppGroup",
                type: "varchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                schema: "AppNotification",
                table: "NotificationWhatsAppGroup");
        }
    }
}
