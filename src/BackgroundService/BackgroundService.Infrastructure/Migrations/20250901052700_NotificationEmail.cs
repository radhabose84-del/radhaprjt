using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificationEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Notification");

            migrationBuilder.AddColumn<bool>(
                name: "IsTable",
                schema: "AppNotification",
                table: "NotificationTemplate",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "NotificationTablePreset",
                schema: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    PresetKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ColumnsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTablePreset", x => x.Id);
                    table.CheckConstraint("CK_TablePresets_ColumnsJson_IsJson", "ISJSON([ColumnsJson]) = 1");
                    table.ForeignKey(
                        name: "FK_NotificationTablePreset_NotificationTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "AppNotification",
                        principalTable: "NotificationTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTablePreset_TemplateId",
                schema: "Notification",
                table: "NotificationTablePreset",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "UX_TablePresets_PresetKey",
                schema: "Notification",
                table: "NotificationTablePreset",
                column: "PresetKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTablePreset",
                schema: "Notification");

            migrationBuilder.DropColumn(
                name: "IsTable",
                schema: "AppNotification",
                table: "NotificationTemplate");
        }
    }
}
