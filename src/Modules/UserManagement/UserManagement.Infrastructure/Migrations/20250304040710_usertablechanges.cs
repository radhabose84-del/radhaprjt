using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class usertablechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "UserType",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<int>(
                name: "DivisionId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserGroup",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    GroupName = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupUsers",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserGroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupUsers_UserGroup_UserGroupId",
                        column: x => x.UserGroupId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroupUsers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AppSecurity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EntityId",
                schema: "AppSecurity",
                table: "Users",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupUsers_UserGroupId",
                schema: "AppSecurity",
                table: "UserGroupUsers",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupUsers_UserId",
                schema: "AppSecurity",
                table: "UserGroupUsers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Entity_EntityId",
                schema: "AppSecurity",
                table: "Users",
                column: "EntityId",
                principalSchema: "AppData",
                principalTable: "Entity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Entity_EntityId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserGroupUsers",
                schema: "AppSecurity");

            migrationBuilder.DropTable(
                name: "UserGroup",
                schema: "AppSecurity");

            migrationBuilder.DropIndex(
                name: "IX_Users_EntityId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "UserType",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "AppSecurity",
                table: "Users",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DivisionId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
