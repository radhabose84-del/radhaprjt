using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class defaultvalueforfirstimeuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
        name: "CreatedAt",
        schema:"AppSecurity",
        table: "PasswordLog",
        type: "datetime",
        nullable: false,
        defaultValueSql: "GETDATE()",
        oldClrType: typeof(DateTime),
        oldNullable: true);
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
                   
        }
    }
}
