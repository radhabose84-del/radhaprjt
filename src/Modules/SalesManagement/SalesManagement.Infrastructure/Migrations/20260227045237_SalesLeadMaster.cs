using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesLeadMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "ValidTo",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ValidFrom",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateTable(
                name: "SalesLead",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: true),
                    ProspectCompanyName = table.Column<string>(type: "varchar(200)", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    ContactName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    EmailId = table.Column<string>(type: "varchar(150)", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    RequirementQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ExpectedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    LeadSourceId = table.Column<int>(type: "int", nullable: true),
                    MarketingPersonId = table.Column<int>(type: "int", nullable: false),
                    InteractionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_SalesLead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesLead_MiscMaster_LeadSourceId",
                        column: x => x.LeadSourceId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesLead_SalesContact_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "Sales",
                        principalTable: "SalesContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_ContactId",
                schema: "Sales",
                table: "SalesLead",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_InteractionDate",
                schema: "Sales",
                table: "SalesLead",
                column: "InteractionDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_LeadSourceId",
                schema: "Sales",
                table: "SalesLead",
                column: "LeadSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_MarketingPersonId",
                schema: "Sales",
                table: "SalesLead",
                column: "MarketingPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_MobileNumber",
                schema: "Sales",
                table: "SalesLead",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_PartyId",
                schema: "Sales",
                table: "SalesLead",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesLead",
                schema: "Sales");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ValidTo",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ValidFrom",
                schema: "Sales",
                table: "SalesItemPriceMaster",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
