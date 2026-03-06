using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeApprovalTargetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalTarget",
                schema: "AppData");

            migrationBuilder.AddColumn<string>(
                name: "Binding",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Binding",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "Value",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.CreateTable(
                name: "ApprovalTarget",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStepDetailId = table.Column<int>(type: "int", nullable: false),
                    Binding = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(200)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalTarget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalTarget_ApprovalStepDetail_ApprovalStepDetailId",
                        column: x => x.ApprovalStepDetailId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalStepDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTarget_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "ApprovalStepDetailId");
        }
    }
}
