using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetMasterIncludeSubGroupId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetSubGroupId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetSubGroupId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetSubGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetSubGroup_AssetSubGroupId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetSubGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetSubGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetSubGroup_AssetSubGroupId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetSubGroupId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropColumn(
                name: "AssetSubGroupId",
                schema: "FixedAsset",
                table: "AssetMaster");
        }
    }
}
