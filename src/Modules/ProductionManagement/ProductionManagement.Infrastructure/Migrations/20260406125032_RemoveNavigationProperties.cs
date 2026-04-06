using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_LotMaster_LotId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_MiscMaster_QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId",
                principalSchema: "Production",
                principalTable: "PackType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.AddColumn<int>(
                name: "PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "QualityStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_LotMaster_LotId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "LotId",
                principalSchema: "Production",
                principalTable: "LotMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_MiscMaster_QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "QualityStatusId",
                principalSchema: "Production",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId",
                principalSchema: "Production",
                principalTable: "PackType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId1",
                principalSchema: "Production",
                principalTable: "PackType",
                principalColumn: "Id");
        }
    }
}
