using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class passwordlogColumnAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
      

        

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "AppSecurity",
                table: "PasswordLog");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "PasswordLog");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "AppSecurity",
                table: "PasswordLog");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "AppSecurity",
                table: "PasswordLog");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "AppSecurity",
                table: "PasswordLog");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "PasswordLog");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "PasswordLog");

       

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           


            

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "PasswordLog",
                type: "nvarchar(max)",
                nullable: true);

       
        }
    }
}
