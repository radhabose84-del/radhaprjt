using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesOfficeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOffice",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOfficeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SalesOrganisationId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Pincode = table.Column<string>(type: "varchar(20)", nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    Email = table.Column<string>(type: "varchar(200)", nullable: true),
                    ResponsibleManager = table.Column<string>(type: "varchar(100)", nullable: true),
                    RegionTerritory = table.Column<string>(type: "varchar(100)", nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_SalesOffice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOffice_SalesOrganisation_SalesOrganisationId",
                        column: x => x.SalesOrganisationId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrganisation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOffice_CityId",
                schema: "Sales",
                table: "SalesOffice",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOffice_SalesOrganisationId",
                schema: "Sales",
                table: "SalesOffice",
                column: "SalesOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOffice_SalesOrganisationId_SalesOfficeName",
                schema: "Sales",
                table: "SalesOffice",
                columns: new[] { "SalesOrganisationId", "SalesOfficeName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOffice",
                schema: "Sales");
        }
    }
}
