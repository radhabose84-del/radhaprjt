using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFirstTimeUser",
                schema: "AppSecurity",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                     migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "Users",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "IsFirstTimeUser",
                schema: "AppSecurity",
                table: "Users",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
