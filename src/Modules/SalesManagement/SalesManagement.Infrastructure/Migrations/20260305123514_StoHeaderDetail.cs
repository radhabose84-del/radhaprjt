using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoHeaderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StoTypeId = table.Column<int>(type: "int", nullable: false),
                    MovementTypeId = table.Column<int>(type: "int", nullable: false),
                    SupplyingPlantId = table.Column<int>(type: "int", nullable: false),
                    SupplyingStorageLocationId = table.Column<int>(type: "int", nullable: false),
                    ReceivingPlantId = table.Column<int>(type: "int", nullable: false),
                    ReceivingStorageLocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_StoHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoHeader_MovementTypeConfig_MovementTypeId",
                        column: x => x.MovementTypeId,
                        principalSchema: "Sales",
                        principalTable: "MovementTypeConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoHeader_StoTypeMaster_StoTypeId",
                        column: x => x.StoTypeId,
                        principalSchema: "Sales",
                        principalTable: "StoTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    TransferPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    LineStatusId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoDetail_MiscMaster_LineStatusId",
                        column: x => x.LineStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoDetail_StoHeader_StoHeaderId",
                        column: x => x.StoHeaderId,
                        principalSchema: "Sales",
                        principalTable: "StoHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoDetail_ItemId",
                schema: "Sales",
                table: "StoDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StoDetail_LineStatusId",
                schema: "Sales",
                table: "StoDetail",
                column: "LineStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StoDetail_StoHeaderId",
                schema: "Sales",
                table: "StoDetail",
                column: "StoHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_DocumentDate",
                schema: "Sales",
                table: "StoHeader",
                column: "DocumentDate");

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_MovementTypeId",
                schema: "Sales",
                table: "StoHeader",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_ReceivingPlantId",
                schema: "Sales",
                table: "StoHeader",
                column: "ReceivingPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_StoNumber",
                schema: "Sales",
                table: "StoHeader",
                column: "StoNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_StoTypeId",
                schema: "Sales",
                table: "StoHeader",
                column: "StoTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoHeader_SupplyingPlantId",
                schema: "Sales",
                table: "StoHeader",
                column: "SupplyingPlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "StoHeader",
                schema: "Sales");
        }
    }
}
