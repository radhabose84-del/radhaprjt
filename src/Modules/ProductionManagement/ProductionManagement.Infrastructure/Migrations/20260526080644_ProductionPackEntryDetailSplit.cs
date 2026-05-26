using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionPackEntryDetailSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionPackEntry_LotId",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "EndPackNo",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "LotId",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "NetWeightPerPack",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "OpeningLooseKgs",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "ProductionKgs",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "Remarks",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "StartPackNo",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "TotalBags",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "TotalNetWeight",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.DropColumn(
                name: "TotalProductionKgs",
                schema: "Production",
                table: "ProductionPackEntry");

            migrationBuilder.CreateTable(
                name: "ProductionPackEntryDetail",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionPackEntryId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    StartPackNo = table.Column<int>(type: "int", nullable: true),
                    EndPackNo = table.Column<int>(type: "int", nullable: true),
                    OpeningLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackEntryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPackEntryDetail_ProductionPackEntry_ProductionPackEntryId",
                        column: x => x.ProductionPackEntryId,
                        principalSchema: "Production",
                        principalTable: "ProductionPackEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntryDetail_LotId",
                schema: "Production",
                table: "ProductionPackEntryDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntryDetail_ProductionPackEntryId",
                schema: "Production",
                table: "ProductionPackEntryDetail",
                column: "ProductionPackEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionPackEntryDetail",
                schema: "Production");

            migrationBuilder.AddColumn<int>(
                name: "EndPackNo",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LotId",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeightPerPack",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningLooseKgs",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionKgs",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartPackNo",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalBags",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNetWeight",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalProductionKgs",
                schema: "Production",
                table: "ProductionPackEntry",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_LotId",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "LotId");
        }
    }
}
