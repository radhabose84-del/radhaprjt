using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adminsecuritytbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.CreateTable(
                name: "AdminSecuritySettings",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PasswordHistoryCount = table.Column<int>(type: "int", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxFailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountAutoUnlockMinutes = table.Column<int>(type: "int", nullable: false),
                    PasswordExpiryDays = table.Column<int>(type: "int", nullable: false),
                    PasswordExpiryAlertDays = table.Column<int>(type: "int", nullable: false),
                    IsTwoFactorAuthenticationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentLogins = table.Column<int>(type: "int", nullable: false),
                    IsForcePasswordChangeOnFirstLogin = table.Column<bool>(type: "bit", nullable: false),
                    PasswordResetCodeExpiryMinutes = table.Column<int>(type: "int", nullable: false),
                    IsCaptchaEnabledOnLogin = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSecuritySettings", x => x.Id);
                });

       
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

          
        }
    }
}
