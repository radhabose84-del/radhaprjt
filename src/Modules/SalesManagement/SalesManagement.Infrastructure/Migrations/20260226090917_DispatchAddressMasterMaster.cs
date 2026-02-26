using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DispatchAddressMasterMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispatchAddressMaster",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchAddressName = table.Column<string>(type: "varchar(150)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "varchar(250)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "varchar(250)", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    PinCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    ContactPerson = table.Column<string>(type: "varchar(120)", nullable: true),
                    MobileNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    Email = table.Column<string>(type: "varchar(254)", nullable: true),
                    GSTIN = table.Column<string>(type: "varchar(15)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 18, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 18, scale: 6, nullable: true),
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
                    table.PrimaryKey("PK_DispatchAddressMaster", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMaster_CityId",
                schema: "Sales",
                table: "DispatchAddressMaster",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMaster_Composite",
                schema: "Sales",
                table: "DispatchAddressMaster",
                columns: new[] { "DispatchAddressName", "CityId", "PinCode" });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMaster_CountryId",
                schema: "Sales",
                table: "DispatchAddressMaster",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMaster_StateId",
                schema: "Sales",
                table: "DispatchAddressMaster",
                column: "StateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchAddressMaster",
                schema: "Sales");
        }
    }
}
