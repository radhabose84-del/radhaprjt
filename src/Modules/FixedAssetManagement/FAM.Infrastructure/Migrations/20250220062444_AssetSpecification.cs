using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetSpecifications",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    ManufactureId = table.Column<int>(type: "int", nullable: false),
                    ManufactureDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SpecificationId = table.Column<int>(type: "int", nullable: false),
                    SpecificationValue = table.Column<string>(type: "varchar(100)", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModelNumber = table.Column<string>(type: "varchar(100)", nullable: true),
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
                    table.PrimaryKey("PK_AssetSpecifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetSpecifications_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetSpecifications_Manufacture_ManufactureId",
                        column: x => x.ManufactureId,
                        principalSchema: "FixedAsset",
                        principalTable: "Manufacture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetSpecifications_SpecificationMaster_SpecificationId",
                        column: x => x.SpecificationId,
                        principalSchema: "FixedAsset",
                        principalTable: "SpecificationMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecifications_AssetId",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecifications_ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                column: "ManufactureId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecifications_SpecificationId",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                column: "SpecificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetSpecifications",
                schema: "FixedAsset");
        }
    }
}
