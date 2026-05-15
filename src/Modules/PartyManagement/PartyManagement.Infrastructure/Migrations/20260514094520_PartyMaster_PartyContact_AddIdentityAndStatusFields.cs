using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyMaster_PartyContact_AddIdentityAndStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternateName",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                schema: "Party",
                table: "PartyMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusControlId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternateEmailId",
                schema: "Party",
                table: "PartyContact",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternateMobileNumber",
                schema: "Party",
                table: "PartyContact",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_StatusControlId",
                schema: "Party",
                table: "PartyMaster",
                column: "StatusControlId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMaster_MiscMaster_StatusControlId",
                schema: "Party",
                table: "PartyMaster",
                column: "StatusControlId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyMaster_MiscMaster_StatusControlId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropIndex(
                name: "IX_PartyMaster_StatusControlId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "AlternateName",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "ShortName",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "StatusControlId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "AlternateEmailId",
                schema: "Party",
                table: "PartyContact");

            migrationBuilder.DropColumn(
                name: "AlternateMobileNumber",
                schema: "Party",
                table: "PartyContact");
        }
    }
}
