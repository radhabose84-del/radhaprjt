using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Manufacture",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(10)", nullable: false),
                    ManufactureName = table.Column<string>(type: "varchar(50)", nullable: false),
                    ManufactureType = table.Column<string>(type: "varchar(10)", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    AddressLine1 = table.Column<string>(type: "varchar(250)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "varchar(250)", nullable: false),
                    PinCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    PersonName = table.Column<string>(type: "varchar(50)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacture", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Manufacture",
                schema: "FixedAsset");
        }
    }
}
