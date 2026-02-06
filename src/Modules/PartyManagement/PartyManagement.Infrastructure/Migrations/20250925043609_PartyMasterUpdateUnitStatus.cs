using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyMasterUpdateUnitStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUpdate",
                schema: "Party",
                table: "PartyMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUpdate",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
