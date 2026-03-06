using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approverbindingColumnsAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverBinding",
                schema: "AppData",
                table: "ApprovalRequestLine");

            migrationBuilder.DropColumn(
                name: "ApproverValue",
                schema: "AppData",
                table: "ApprovalRequestLine");

            migrationBuilder.AddColumn<string>(
                name: "ApproverBinding",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApproverValue",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverBinding",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.DropColumn(
                name: "ApproverValue",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.AddColumn<string>(
                name: "ApproverBinding",
                schema: "AppData",
                table: "ApprovalRequestLine",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApproverValue",
                schema: "AppData",
                table: "ApprovalRequestLine",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");
        }
    }
}
