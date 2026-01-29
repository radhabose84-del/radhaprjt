using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class defaultcolumnremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Default",
                schema: "AppSecurity",
                table: "UserUnit");

            migrationBuilder.DropColumn(
                name: "Default",
                schema: "AppSecurity",
                table: "UserCompany");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Default",
                schema: "AppSecurity",
                table: "UserUnit",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Default",
                schema: "AppSecurity",
                table: "UserCompany",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
