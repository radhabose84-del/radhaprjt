using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesAgreementHeader_AddPoFieldsUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentPOAttachment",
                schema: "Sales",
                table: "SalesAgreementHeader",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPoRefno",
                schema: "Sales",
                table: "SalesAgreementHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Sales",
                table: "SalesAgreementHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementHeader_UnitId",
                schema: "Sales",
                table: "SalesAgreementHeader",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesAgreementHeader_UnitId",
                schema: "Sales",
                table: "SalesAgreementHeader");

            migrationBuilder.DropColumn(
                name: "AgentPOAttachment",
                schema: "Sales",
                table: "SalesAgreementHeader");

            migrationBuilder.DropColumn(
                name: "CustomerPoRefno",
                schema: "Sales",
                table: "SalesAgreementHeader");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Sales",
                table: "SalesAgreementHeader");
        }
    }
}
