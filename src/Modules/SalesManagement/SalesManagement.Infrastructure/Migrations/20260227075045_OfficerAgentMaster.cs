using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OfficerAgentMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfficerAgent",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    MarketingOfficerId = table.Column<int>(type: "int", nullable: false),
                    ValidityFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidityTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficerAgent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficerAgent_MarketingOfficer_MarketingOfficerId",
                        column: x => x.MarketingOfficerId,
                        principalSchema: "Sales",
                        principalTable: "MarketingOfficer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfficerAgent_AgentId",
                schema: "Sales",
                table: "OfficerAgent",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficerAgent_MarketingOfficerId",
                schema: "Sales",
                table: "OfficerAgent",
                column: "MarketingOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficerAgent_ValidityFrom_ValidityTo",
                schema: "Sales",
                table: "OfficerAgent",
                columns: new[] { "ValidityFrom", "ValidityTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfficerAgent",
                schema: "Sales");
        }
    }
}
