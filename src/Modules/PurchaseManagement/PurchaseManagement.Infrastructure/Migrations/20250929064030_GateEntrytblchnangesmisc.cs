using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateEntrytblchnangesmisc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PODate",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "PoDate");

            migrationBuilder.RenameColumn(
                name: "POCreatedBy",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "PoCreatedBy");

            migrationBuilder.AlterColumn<string>(
                name: "GSTNumber",
                schema: "Purchase",
                table: "GateEntryDetail",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryHeader_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                column: "PoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryHeader_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                column: "PoTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryHeader_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.DropIndex(
                name: "IX_GateEntryHeader_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.RenameColumn(
                name: "PoDate",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "PODate");

            migrationBuilder.RenameColumn(
                name: "PoCreatedBy",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "POCreatedBy");

            migrationBuilder.AlterColumn<string>(
                name: "GSTNumber",
                schema: "Purchase",
                table: "GateEntryDetail",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true);
        }
    }
}
