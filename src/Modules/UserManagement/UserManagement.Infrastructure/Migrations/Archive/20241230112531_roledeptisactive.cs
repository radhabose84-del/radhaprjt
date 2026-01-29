using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class roledeptisactive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                    migrationBuilder.AlterColumn<bool>(
                    name: "IsActive",
                    schema: "AppData",
                    table: "Department",
                    type: "bit",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "tinyint"); 

                    migrationBuilder.AlterColumn<bool>(
                    name: "IsActive",
                    schema: "AppSecurity",
                    table: "UserRole",
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
