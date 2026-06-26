using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiCompanyCoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlobalAccountId",
                schema: "Finance",
                table: "GlAccountMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompanyRestricted",
                schema: "Finance",
                table: "GlAccountMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                schema: "Finance",
                table: "GlAccountMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocalOverride",
                schema: "Finance",
                table: "GlAccountMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_GlobalAccountId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "GlobalAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_GlAccountMaster_GlAccountMaster_GlobalAccountId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "GlobalAccountId",
                principalSchema: "Finance",
                principalTable: "GlAccountMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlAccountMaster_GlAccountMaster_GlobalAccountId",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropIndex(
                name: "IX_GlAccountMaster_GlobalAccountId",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropColumn(
                name: "GlobalAccountId",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropColumn(
                name: "IsCompanyRestricted",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropColumn(
                name: "IsLocalOverride",
                schema: "Finance",
                table: "GlAccountMaster");
        }
    }
}
