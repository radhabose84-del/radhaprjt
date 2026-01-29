using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocationRelationtoSubLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubLocation_Location_LocationId",
                schema: "FixedAsset",
                table: "SubLocation");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubLocation_Location_LocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                column: "LocationId",
                principalSchema: "FixedAsset",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubLocation_Location_LocationId",
                schema: "FixedAsset",
                table: "SubLocation");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_SubLocation_Location_LocationId",
                schema: "FixedAsset",
                table: "SubLocation",
                column: "LocationId",
                principalSchema: "FixedAsset",
                principalTable: "Location",
                principalColumn: "Id");
        }
    }
}
