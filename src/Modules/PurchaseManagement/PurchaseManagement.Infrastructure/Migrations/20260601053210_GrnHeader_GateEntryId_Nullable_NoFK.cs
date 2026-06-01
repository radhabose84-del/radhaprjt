using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrnHeader_GateEntryId_Nullable_NoFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrnHeader_GateEntryHeader_GateEntryId",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropIndex(
                name: "IX_GrnHeader_GateEntryId",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.AlterColumn<int>(
                name: "GateEntryId",
                schema: "Purchase",
                table: "GrnHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GateEntryId",
                schema: "Purchase",
                table: "GrnHeader",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrnHeader_GateEntryId",
                schema: "Purchase",
                table: "GrnHeader",
                column: "GateEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_GrnHeader_GateEntryHeader_GateEntryId",
                schema: "Purchase",
                table: "GrnHeader",
                column: "GateEntryId",
                principalSchema: "Purchase",
                principalTable: "GateEntryHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
