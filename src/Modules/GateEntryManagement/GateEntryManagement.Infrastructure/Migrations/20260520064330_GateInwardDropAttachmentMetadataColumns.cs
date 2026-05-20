using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardDropAttachmentMetadataColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
