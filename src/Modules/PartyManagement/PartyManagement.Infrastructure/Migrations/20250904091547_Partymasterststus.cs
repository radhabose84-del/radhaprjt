using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Partymasterststus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
