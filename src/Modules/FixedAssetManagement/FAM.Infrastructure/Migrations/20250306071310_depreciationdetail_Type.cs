using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class depreciationdetail_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISLocked",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                newName: "IsLocked");

            migrationBuilder.AddColumn<string>(
                name: "DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "");
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropColumn(
                name: "DepreciationType",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.RenameColumn(
                name: "IsLocked",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                newName: "ISLocked");
        }
    }
}
