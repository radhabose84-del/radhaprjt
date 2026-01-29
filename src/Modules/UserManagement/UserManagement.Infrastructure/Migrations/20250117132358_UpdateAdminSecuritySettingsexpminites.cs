using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminSecuritySettingsexpminites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
           

        migrationBuilder.AlterColumn<int>(
    name: "PasswordResetCodeExpiryMinutes",
    schema: "AppSecurity",
    table: "AdminSecuritySettings",
    type: "int", // New type
    nullable: false,
    oldClrType: typeof(byte), // Old type
    oldType: "tinyint" // Original database type
);


          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}
