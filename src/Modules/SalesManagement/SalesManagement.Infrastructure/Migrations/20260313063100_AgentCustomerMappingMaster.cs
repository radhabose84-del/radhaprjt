using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgentCustomerMappingMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentCustomerMapping",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    SubAgentId = table.Column<int>(type: "int", nullable: true),
                    SalesSegmentId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDefaultAgent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AgentCustomerMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCustomerMapping_SalesSegment_SalesSegmentId",
                        column: x => x.SalesSegmentId,
                        principalSchema: "Sales",
                        principalTable: "SalesSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCustomerMapping_AgentId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCustomerMapping_CustomerId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCustomerMapping_CustomerId_IsDefaultAgent",
                schema: "Sales",
                table: "AgentCustomerMapping",
                columns: new[] { "CustomerId", "IsDefaultAgent" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCustomerMapping_SalesSegmentId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                column: "SalesSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCustomerMapping_SubAgentId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                column: "SubAgentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentCustomerMapping",
                schema: "Sales");
        }
    }
}
