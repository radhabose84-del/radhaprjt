using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesContactMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesContact",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(15)", nullable: false),
                    ContactTypeId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_SalesContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesContact_MiscMaster_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesContact_ContactName",
                schema: "Sales",
                table: "SalesContact",
                column: "ContactName");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContact_ContactTypeId",
                schema: "Sales",
                table: "SalesContact",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContact_MobileNumber",
                schema: "Sales",
                table: "SalesContact",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContact_PartyId",
                schema: "Sales",
                table: "SalesContact",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesContact",
                schema: "Sales");
        }
    }
}
