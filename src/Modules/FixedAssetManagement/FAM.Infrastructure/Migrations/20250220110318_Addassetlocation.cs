using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addassetlocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "subLocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                schema: "FixedAsset",
                table: "Location",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetLocation",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SubLocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetLocation_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "FixedAsset",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetLocation_SubLocation_SubLocationId",
                        column: x => x.SubLocationId,
                        principalSchema: "FixedAsset",
                        principalTable: "SubLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubLocation_subLocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                column: "subLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocationId",
                schema: "FixedAsset",
                table: "Location",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetLocation_LocationId",
                schema: "FixedAsset",
                table: "AssetLocation",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetLocation_SubLocationId",
                schema: "FixedAsset",
                table: "AssetLocation",
                column: "SubLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Location_LocationId",
                schema: "FixedAsset",
                table: "Location",
                column: "LocationId",
                principalSchema: "FixedAsset",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubLocation_SubLocation_subLocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                column: "subLocationId",
                principalSchema: "FixedAsset",
                principalTable: "SubLocation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_Location_LocationId",
                schema: "FixedAsset",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_SubLocation_SubLocation_subLocationId",
                schema: "FixedAsset",
                table: "SubLocation");

            migrationBuilder.DropTable(
                name: "AssetLocation",
                schema: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_SubLocation_subLocationId",
                schema: "FixedAsset",
                table: "SubLocation");

            migrationBuilder.DropIndex(
                name: "IX_Location_LocationId",
                schema: "FixedAsset",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "subLocationId",
                schema: "FixedAsset",
                table: "SubLocation");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "FixedAsset",
                table: "Location");
        }
    }
}
