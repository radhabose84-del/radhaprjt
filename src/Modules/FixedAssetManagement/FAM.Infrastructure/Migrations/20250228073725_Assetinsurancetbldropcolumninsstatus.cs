using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Assetinsurancetbldropcolumninsstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.DropColumn(
            name: "InsuranceStatus",
            table: "AssetInsurance",
            schema: "FixedAsset"
                 );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.AddColumn<byte>(
                name: "InsuranceStatus",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0  );
        }
    }
}
