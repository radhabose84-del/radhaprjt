using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDepreciationDetailIsactiveIsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "FixedAsset",
                table: "DepreciationDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
