using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class depreciationdetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {  
            migrationBuilder.CreateTable(
                name: "DepreciationDetail",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Finyear = table.Column<string>(type: "varchar(10)", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetGroupId = table.Column<int>(type: "int", nullable: false),
                    AssetValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CapitalizationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ResidualValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsefulLifeDays = table.Column<int>(type: "int", nullable: false),
                    DaysOpening = table.Column<int>(type: "int", nullable: false),
                    DaysUsed = table.Column<int>(type: "int", nullable: false),
                    OpeningValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DepreciationValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ClosingValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ISLocked = table.Column<bool>(type: "bit", nullable: false),                    
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
                    table.PrimaryKey("PK_DepreciationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepreciationDetail_AssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciationDetail_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
       
            migrationBuilder.CreateIndex(
                name: "IX_DepreciationDetail_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                column: "AssetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationDetail_AssetId",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                column: "AssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {          
            migrationBuilder.DropTable(
                name: "DepreciationDetail",
                schema: "FixedAsset");
         
        }
    }
}
