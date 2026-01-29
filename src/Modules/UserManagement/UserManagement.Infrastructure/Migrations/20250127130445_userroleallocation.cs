using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userroleallocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                table: "UserRoleAllocation",
                schema: "AppSecurity");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte>(
                name: "IsActive",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                table: "UserRoleAllocation",
                schema: "AppSecurity",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
