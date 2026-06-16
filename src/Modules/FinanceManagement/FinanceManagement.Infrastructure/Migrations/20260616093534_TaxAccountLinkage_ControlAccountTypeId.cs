using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaxAccountLinkage_ControlAccountTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "ControlAccountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "ControlAccountTypeId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropIndex(
                name: "IX_TaxAccountLinkage_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage");
        }
    }
}
