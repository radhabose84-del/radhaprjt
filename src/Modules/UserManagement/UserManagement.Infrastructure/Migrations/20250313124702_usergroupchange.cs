using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class usergroupchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGroupUsers",
                schema: "AppSecurity");

            migrationBuilder.AddColumn<byte>(
                name: "Default",
                schema: "AppSecurity",
                table: "UserUnit",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "UserGroupId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Default",
                schema: "AppSecurity",
                table: "UserCompany",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGroupId",
                schema: "AppSecurity",
                table: "Users",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserGroup_UserGroupId",
                schema: "AppSecurity",
                table: "Users",
                column: "UserGroupId",
                principalSchema: "AppSecurity",
                principalTable: "UserGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserGroup_UserGroupId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserGroupId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Default",
                schema: "AppSecurity",
                table: "UserUnit");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Default",
                schema: "AppSecurity",
                table: "UserCompany");

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
        }
    }
}
