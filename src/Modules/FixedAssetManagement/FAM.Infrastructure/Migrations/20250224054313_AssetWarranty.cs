using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {           
            migrationBuilder.CreateTable(
                name: "AssetWarranty",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    WarrantyType = table.Column<int>(type: "int", nullable: false),
                    WarrantyProvider = table.Column<string>(type: "varchar(250)", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ContactPerson = table.Column<string>(type: "varchar(50)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    Document = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ServiceCountryId = table.Column<int>(type: "int", nullable: false),
                    ServiceStateId = table.Column<int>(type: "int", nullable: false),
                    ServiceCityId = table.Column<int>(type: "int", nullable: false),
                    ServiceAddressLine1 = table.Column<string>(type: "varchar(250)", nullable: false),
                    ServiceAddressLine2 = table.Column<string>(type: "varchar(250)", nullable: true),
                    ServicePinCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    ServiceContactPerson = table.Column<string>(type: "varchar(50)", nullable: false),
                    ServiceMobileNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    ServiceEmail = table.Column<string>(type: "varchar(100)", nullable: false),
                    ServiceClaimProcessDescription = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ServiceLastClaimDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ServiceClaimStatus = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AssetWarranty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetWarranty_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClaimStatus_Misc",
                        column: x => x.ServiceClaimStatus,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyType_Misc",
                        column: x => x.WarrantyType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
         
            migrationBuilder.CreateIndex(
                name: "IX_AssetWarranty_AssetId",
                schema: "FixedAsset",
                table: "AssetWarranty",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetWarranty_ServiceClaimStatus",
                schema: "FixedAsset",
                table: "AssetWarranty",
                column: "ServiceClaimStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AssetWarranty_WarrantyType",
                schema: "FixedAsset",
                table: "AssetWarranty",
                column: "WarrantyType");
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {           

            migrationBuilder.DropTable(
                name: "AssetWarranty",
                schema: "FixedAsset");          
        }
    }
}
