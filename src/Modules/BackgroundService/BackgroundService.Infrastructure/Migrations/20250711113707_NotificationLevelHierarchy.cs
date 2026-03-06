using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificationLevelHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                type: "Varchar(Max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "Varchar(Max)");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "AppNotification",
                table: "NotificationLevelHierarchy",
                type: "Varchar(Max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "Varchar(Max)",
                oldNullable: true);
        }
    }
}
