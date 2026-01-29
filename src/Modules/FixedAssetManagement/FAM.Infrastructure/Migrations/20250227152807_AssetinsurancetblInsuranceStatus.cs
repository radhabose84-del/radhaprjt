using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetinsurancetblInsuranceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "InsuranceStatus",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InsuranceStatus",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
