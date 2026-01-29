using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changemiscidandDatatype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeMasterId",
                schema: "FixedAsset",
                table: "MiscMaster");

            migrationBuilder.RenameColumn(
                name: "MiscTypeMasterId",
                schema: "FixedAsset",
                table: "MiscMaster",
                newName: "MiscTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_MiscMaster_MiscTypeMasterId",
                schema: "FixedAsset",
                table: "MiscMaster",
                newName: "IX_MiscMaster_MiscTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "MiscTypeCode",
                schema: "FixedAsset",
                table: "MiscTypeMaster",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AddForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                column: "MiscTypeId",
                principalSchema: "FixedAsset",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeId",
                schema: "FixedAsset",
                table: "MiscMaster");

            migrationBuilder.RenameColumn(
                name: "MiscTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                newName: "MiscTypeMasterId");

            migrationBuilder.RenameIndex(
                name: "IX_MiscMaster_MiscTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                newName: "IX_MiscMaster_MiscTypeMasterId");

            migrationBuilder.AlterColumn<string>(
                name: "MiscTypeCode",
                schema: "FixedAsset",
                table: "MiscTypeMaster",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AddForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeMasterId",
                schema: "FixedAsset",
                table: "MiscMaster",
                column: "MiscTypeMasterId",
                principalSchema: "FixedAsset",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
