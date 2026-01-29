using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class companysetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySetting",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PasswordHistoryCount = table.Column<int>(type: "int", nullable: false),
                    SessionTimeout = table.Column<int>(type: "int", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AutoReleaseTime = table.Column<int>(type: "int", nullable: false),
                    PasswordExpiryDays = table.Column<int>(type: "int", nullable: false),
                    PasswordExpiryAlert = table.Column<int>(type: "int", nullable: false),
                    TwoFactorAuth = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentLogins = table.Column<int>(type: "int", nullable: false),
                    ForgotPasswordCodeExpiry = table.Column<int>(type: "int", nullable: false),
                    CaptchaOnLogin = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<int>(type: "int", nullable: false),
                    FinancialYear = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_CompanySetting", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySetting",
                schema: "AppData");
        }
    }
}
