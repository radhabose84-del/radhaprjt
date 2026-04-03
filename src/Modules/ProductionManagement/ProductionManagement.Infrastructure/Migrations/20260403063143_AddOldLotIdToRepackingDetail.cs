using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOldLotIdToRepackingDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndPackNo",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OldLotId",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartPackNo",
                schema: "Production",
                table: "RepackingDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndPackNo",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "OldLotId",
                schema: "Production",
                table: "RepackingDetail");

            migrationBuilder.DropColumn(
                name: "StartPackNo",
                schema: "Production",
                table: "RepackingDetail");
        }
    }
}
