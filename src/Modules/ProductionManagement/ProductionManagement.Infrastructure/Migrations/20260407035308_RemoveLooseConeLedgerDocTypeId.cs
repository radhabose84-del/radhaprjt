using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLooseConeLedgerDocTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepackingHeader_MiscMaster_TypeId",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropIndex(
                name: "IX_RepackingHeader_TypeId",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropColumn(
                name: "TypeId",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropColumn(
                name: "DocType",
                schema: "Production",
                table: "LooseConeLedger");

            migrationBuilder.AddColumn<int>(
                name: "DocTypeId",
                schema: "Production",
                table: "LooseConeLedger",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocTypeId",
                schema: "Production",
                table: "LooseConeLedger");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                schema: "Production",
                table: "RepackingHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                schema: "Production",
                table: "LooseConeLedger",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_TypeId",
                schema: "Production",
                table: "RepackingHeader",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepackingHeader_MiscMaster_TypeId",
                schema: "Production",
                table: "RepackingHeader",
                column: "TypeId",
                principalSchema: "Production",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
