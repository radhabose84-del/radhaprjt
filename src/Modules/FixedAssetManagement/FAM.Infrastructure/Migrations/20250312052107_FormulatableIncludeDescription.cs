using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FormulatableIncludeDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "FixedAsset",
                table: "FormulaTable",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "FixedAsset",
                table: "FormulaTable",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationDetail_DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                column: "DepreciationType");

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationDetail_MiscMaster_DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                column: "DepreciationType",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationDetail_MiscMaster_DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropIndex(
                name: "IX_DepreciationDetail_DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "FixedAsset",
                table: "FormulaTable");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "FixedAsset",
                table: "FormulaTable");

            migrationBuilder.AlterColumn<string>(
                name: "DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
