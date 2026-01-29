using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetAmcTablemigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetAmc",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    VendorCode = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    VendorPhone = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    VendorEmail = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CoverageType = table.Column<int>(type: "int", nullable: false),
                    FreeServiceCount = table.Column<int>(type: "int", nullable: true),
                    RenewalStatus = table.Column<int>(type: "int", nullable: false),
                    RenewedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_AssetAmc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAmc_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetAmc_MiscMaster_CoverageType",
                        column: x => x.CoverageType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetAmc_MiscMaster_RenewalStatus",
                        column: x => x.RenewalStatus,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAmc_AssetId",
                schema: "FixedAsset",
                table: "AssetAmc",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAmc_CoverageType",
                schema: "FixedAsset",
                table: "AssetAmc",
                column: "CoverageType");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAmc_RenewalStatus",
                schema: "FixedAsset",
                table: "AssetAmc",
                column: "RenewalStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAmc",
                schema: "FixedAsset");
        }
    }
}
