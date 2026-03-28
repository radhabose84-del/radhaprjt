using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComplaintResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintResolution",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintHeaderId = table.Column<int>(type: "int", nullable: false),
                    ResolutionTypeId = table.Column<int>(type: "int", nullable: false),
                    ResolutionSummary = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    ReturnLocationId = table.Column<int>(type: "int", nullable: true),
                    ReturnStatusId = table.Column<int>(type: "int", nullable: true),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    FinanceReference = table.Column<string>(type: "varchar(100)", nullable: true),
                    ReplacementQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    DispatchReference = table.Column<string>(type: "varchar(100)", nullable: true),
                    ActionDescription = table.Column<string>(type: "nvarchar(2000)", nullable: true),
                    ClosureStatusId = table.Column<int>(type: "int", nullable: true),
                    ClosureRemarks = table.Column<string>(type: "nvarchar(2000)", nullable: true),
                    ResolvedBy = table.Column<int>(type: "int", nullable: true),
                    ResolvedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedBy = table.Column<int>(type: "int", nullable: true),
                    ClosedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintResolution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintResolution_ComplaintHeader_ComplaintHeaderId",
                        column: x => x.ComplaintHeaderId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintResolution_MiscMaster_ClosureStatusId",
                        column: x => x.ClosureStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintResolution_MiscMaster_ResolutionTypeId",
                        column: x => x.ResolutionTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintResolution_MiscMaster_ReturnLocationId",
                        column: x => x.ReturnLocationId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintResolution_MiscMaster_ReturnStatusId",
                        column: x => x.ReturnStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintResolution_ClosureStatusId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ClosureStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintResolution_ComplaintHeaderId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ComplaintHeaderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintResolution_ResolutionTypeId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ResolutionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintResolution_ReturnLocationId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ReturnLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintResolution_ReturnStatusId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ReturnStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintResolution",
                schema: "Sales");
        }
    }
}
