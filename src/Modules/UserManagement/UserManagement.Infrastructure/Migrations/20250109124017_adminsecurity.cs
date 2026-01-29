using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adminsecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<bool>(
                    name: "IsTwoFactorAuthenticationEnabled",
                    schema: "AppSecurity",
                    table: "AdminSecuritySettings",
                    type: "bit",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "tinyint"); 

                    migrationBuilder.AlterColumn<bool>(
                    name: "IsForcePasswordChangeOnFirstLogin",
                    schema: "AppSecurity",
                    table: "AdminSecuritySettings",
                    type: "bit",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "tinyint"); 

                       migrationBuilder.AlterColumn<bool>(
                    name: "PasswordResetCodeExpiryMinutes",
                    schema: "AppSecurity",
                    table: "AdminSecuritySettings",
                    type: "bit",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "tinyint"); 

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
