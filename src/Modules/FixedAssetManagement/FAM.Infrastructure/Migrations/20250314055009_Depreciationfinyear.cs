using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Depreciationfinyear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Finyear",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Finyear",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
