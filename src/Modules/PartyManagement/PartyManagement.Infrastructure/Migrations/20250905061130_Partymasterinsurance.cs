using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Partymasterinsurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceLimit",
                schema: "Party",
                table: "PartyMaster",
                type: "decimal(18,3)",
                nullable: true,
                defaultValue: 0.000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceLimit",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
