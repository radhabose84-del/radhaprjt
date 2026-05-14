using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserSignature_AddRfqFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                schema: "AppData",
                table: "UserSignature",
                type: "varchar(500)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                schema: "AppData",
                table: "UserSignature",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                schema: "AppData",
                table: "UserSignature",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                schema: "AppData",
                table: "UserSignature",
                type: "varchar(200)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                schema: "AppData",
                table: "UserSignature");

            migrationBuilder.DropColumn(
                name: "FileSize",
                schema: "AppData",
                table: "UserSignature");

            migrationBuilder.DropColumn(
                name: "FileType",
                schema: "AppData",
                table: "UserSignature");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                schema: "AppData",
                table: "UserSignature");
        }
    }
}
