using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class baseentitymodify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "UserRole",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "UserRole",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "UserRole",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "UserRole",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "UserRole",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Unit",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Unit",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Unit",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Unit",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Unit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "State",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Entity",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Entity",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Entity",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Entity",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Entity",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Division",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Division",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Division",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Division",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Division",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Department",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Department",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Department",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Department",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Department",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Country",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "CompanySetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Company",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "City",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "AdminSecuritySettings",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "AdminSecuritySettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "State");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "RoleEntitlements");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "PasswordComplexityRule");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Entity");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Division");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "CompanySetting");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "City");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppSecurity",
                table: "AdminSecuritySettings");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "UserRole",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "UserRole",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "UserRole",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "UserRole",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Unit",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Unit",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Unit",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Unit",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppSecurity",
                table: "PasswordComplexityRule",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Division",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Division",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Division",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Division",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Department",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Department",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Department",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Department",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<byte>(
                name: "IsActive",
                schema: "AppSecurity",
                table: "AdminSecuritySettings",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
