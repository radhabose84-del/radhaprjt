using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Assetinsurancetbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetInsurance",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    PolicyNo = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Insuranceperiod = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PolicyAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    VendorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RenewalStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    RenewedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InsuranceStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInsurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetInsurance_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetInsurance_AssetId",
                schema: "FixedAsset",
                table: "AssetInsurance",
                column: "AssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetInsurance",
                schema: "FixedAsset");
        }
    }
}
