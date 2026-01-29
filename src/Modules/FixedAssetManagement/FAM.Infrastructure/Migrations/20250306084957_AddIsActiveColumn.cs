using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.AddColumn<bool>(
        name: "IsActive",
        schema: "FixedAsset",
        table: "DepreciationDetail",
        type: "bit",
        nullable: false,
        defaultValue: true); // Default value set to true
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "FixedAsset",
                table: "DepreciationDetail");

        }
    }
}
