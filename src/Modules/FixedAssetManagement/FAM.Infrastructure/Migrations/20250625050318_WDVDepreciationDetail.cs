using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WDVDepreciationDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WDVDepreciationDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    FinYear = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AssetGroupId = table.Column<int>(type: "int", nullable: false),
                    AssetSubGroupId = table.Column<int>(type: "int", nullable: false),
                    DepreciationPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LessThan180DaysValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoreThan180DaysValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeletionValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalDepreciationValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WDVDepreciationValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGainLossValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsLocked = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WDVDepreciationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WDVDepreciationDetail_AssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WDVDepreciationDetail_AssetSubGroup_AssetSubGroupId",
                        column: x => x.AssetSubGroupId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetSubGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WDVDepreciationDetail_AssetGroupId",
                table: "WDVDepreciationDetail",
                column: "AssetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WDVDepreciationDetail_AssetSubGroupId",
                table: "WDVDepreciationDetail",
                column: "AssetSubGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WDVDepreciationDetail");
        }
    }
}
