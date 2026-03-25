using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackMaterialToPackType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackMaterialId",
                schema: "Production",
                table: "PackType",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackType_PackMaterialId",
                schema: "Production",
                table: "PackType",
                column: "PackMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_PackType_MiscMaster_PackMaterialId",
                schema: "Production",
                table: "PackType",
                column: "PackMaterialId",
                principalSchema: "Production",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackType_MiscMaster_PackMaterialId",
                schema: "Production",
                table: "PackType");

            migrationBuilder.DropIndex(
                name: "IX_PackType_PackMaterialId",
                schema: "Production",
                table: "PackType");

            migrationBuilder.DropColumn(
                name: "PackMaterialId",
                schema: "Production",
                table: "PackType");
        }
    }
}
