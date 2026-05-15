using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserSignature_DropBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                schema: "AppData",
                table: "UserSignature");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                schema: "AppData",
                table: "UserSignature");

            migrationBuilder.DropColumn(
                name: "SignatureImage",
                schema: "AppData",
                table: "UserSignature");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                schema: "AppData",
                table: "UserSignature",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FileSizeBytes",
                schema: "AppData",
                table: "UserSignature",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "SignatureImage",
                schema: "AppData",
                table: "UserSignature",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
