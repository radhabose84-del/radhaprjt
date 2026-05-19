using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardWeightsIntAndAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TareWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NetWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GrossWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "nvarchar(260)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFilePath",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AttachmentFileSize",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileType",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentOriginalFileName",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "nvarchar(260)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "AttachmentFilePath",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "AttachmentFileSize",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "AttachmentFileType",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "AttachmentOriginalFileName",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.AlterColumn<decimal>(
                name: "TareWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GrossWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
