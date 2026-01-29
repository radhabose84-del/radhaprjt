using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DepreciationRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<decimal>(
                name: "UsefulLife",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "DepreciationRate",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetValue",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {       
            migrationBuilder.AlterColumn<int>(
                name: "UsefulLife",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");
        }
    }
}
