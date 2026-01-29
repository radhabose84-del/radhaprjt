using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSessionTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Users_UserId",
                schema: "AppSecurity",
                table: "UserSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSessions",
                schema: "AppSecurity",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_UserId",
                schema: "AppSecurity",
                table: "UserSessions");

            migrationBuilder.RenameTable(
                name: "UserSessions",
                schema: "AppSecurity",
                newName: "UserSession");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActivity",
                table: "UserSession",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "JwtId",
                table: "UserSession",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                table: "UserSession",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserSession",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserSession",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "UserSession",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSession",
                table: "UserSession",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSession",
                table: "UserSession");

            migrationBuilder.RenameTable(
                name: "UserSession",
                newName: "UserSessions",
                newSchema: "AppSecurity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActivity",
                schema: "AppSecurity",
                table: "UserSessions",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "JwtId",
                schema: "AppSecurity",
                table: "UserSessions",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "UserSessions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                schema: "AppSecurity",
                table: "UserSessions",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "AppSecurity",
                table: "UserSessions",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                schema: "AppSecurity",
                table: "UserSessions",
                type: "varchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSessions",
                schema: "AppSecurity",
                table: "UserSessions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                schema: "AppSecurity",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Users_UserId",
                schema: "AppSecurity",
                table: "UserSessions",
                column: "UserId",
                principalSchema: "AppSecurity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
