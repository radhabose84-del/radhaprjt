using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dutymastercoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_DutyMaster_CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster",
                column: "CountryOfOriginApplicability");

            migrationBuilder.AddForeignKey(
                name: "FK_DutyMaster_MiscMaster_CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster",
                column: "CountryOfOriginApplicability",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DutyMaster_MiscMaster_CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster");

            migrationBuilder.DropIndex(
                name: "IX_DutyMaster_CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster");

            migrationBuilder.AlterColumn<bool>(
                name: "CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
