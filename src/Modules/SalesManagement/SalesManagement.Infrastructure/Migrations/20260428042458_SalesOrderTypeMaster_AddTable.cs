using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderTypeMaster_AddTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOrderTypeMaster",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoTypeId = table.Column<int>(type: "int", nullable: false),
                    TaxTypeId = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    AllowsDispatch = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiresValidity = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowZeroPrice = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MinPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    MaxPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    MaxQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    AllowPriceOverride = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OverrideLimitPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 6, nullable: true),
                    ApprovalRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CurrencyRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowIGST = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CountryMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DefaultCurrencyId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_SalesOrderTypeMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderTypeMaster_MiscMaster_SoTypeId",
                        column: x => x.SoTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTypeMaster_DefaultCurrencyId",
                schema: "Sales",
                table: "SalesOrderTypeMaster",
                column: "DefaultCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTypeMaster_TaxTypeId",
                schema: "Sales",
                table: "SalesOrderTypeMaster",
                column: "TaxTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_SalesOrderTypeMaster_SoType_TaxType",
                schema: "Sales",
                table: "SalesOrderTypeMaster",
                columns: new[] { "SoTypeId", "TaxTypeId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderTypeMaster",
                schema: "Sales");
        }
    }
}
