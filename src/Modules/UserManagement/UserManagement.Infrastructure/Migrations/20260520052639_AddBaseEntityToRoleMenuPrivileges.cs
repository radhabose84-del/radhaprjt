using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityToRoleMenuPrivileges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                type: "varchar(255)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege");
        }
    }
}
