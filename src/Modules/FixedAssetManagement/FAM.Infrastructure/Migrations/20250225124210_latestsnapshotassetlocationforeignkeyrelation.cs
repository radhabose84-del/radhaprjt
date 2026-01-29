using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class latestsnapshotassetlocationforeignkeyrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_Location_LocationId",
                schema: "FixedAsset",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_SubLocation_SubLocation_subLocationId",
                schema: "FixedAsset",
                table: "SubLocation");

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

            migrationBuilder.CreateIndex(
                name: "IX_AssetLocation_AssetId",
                schema: "FixedAsset",
                table: "AssetLocation",
                column: "AssetId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLocation_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetLocation",
                column: "AssetId",
                principalSchema: "FixedAsset",
                principalTable: "AssetMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetLocation_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetLocation");

            migrationBuilder.DropIndex(
                name: "IX_AssetLocation_AssetId",
                schema: "FixedAsset",
                table: "AssetLocation");

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
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Location_LocationId",
                schema: "FixedAsset",
                table: "Location",
                column: "LocationId",
                principalSchema: "FixedAsset",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubLocation_SubLocation_subLocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                column: "subLocationId",
                principalSchema: "FixedAsset",
                principalTable: "SubLocation",
                principalColumn: "Id");
        }
    }
}
