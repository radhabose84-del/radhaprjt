using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetAdditionalCostmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.CreateTable(
                name: "AssetAdditionalCost",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetSourceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    JournalNo = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CostType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAdditionalCost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAdditionalCost_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetAdditionalCost_AssetSource_AssetSourceId",
                        column: x => x.AssetSourceId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetAdditionalCost_MiscMaster_CostType",
                        column: x => x.CostType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });


            migrationBuilder.CreateIndex(
                name: "IX_AssetAdditionalCost_AssetId",
                schema: "FixedAsset",
                table: "AssetAdditionalCost",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAdditionalCost_AssetSourceId",
                schema: "FixedAsset",
                table: "AssetAdditionalCost",
                column: "AssetSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAdditionalCost_CostType",
                schema: "FixedAsset",
                table: "AssetAdditionalCost",
                column: "CostType");
          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
               migrationBuilder.DropTable(
                name: "AssetAdditionalCost",
                schema: "FixedAsset");
          
           
        }
    }
}
