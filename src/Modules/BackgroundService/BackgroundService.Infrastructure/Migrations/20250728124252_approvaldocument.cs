using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvaldocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remark",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "Varchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApprovalDocument",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalRequestId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalDocument_ApprovalRequest_ApprovalRequestId",
                        column: x => x.ApprovalRequestId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalRequest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDocument_ApprovalRequestId",
                schema: "AppData",
                table: "ApprovalDocument",
                column: "ApprovalRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalDocument",
                schema: "AppData");

            migrationBuilder.DropColumn(
                name: "Remark",
                schema: "AppData",
                table: "ApprovalRequest");
        }
    }
}
