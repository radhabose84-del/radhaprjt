using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyMasterUnitMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.AddColumn<bool>(
                name: "IsPortalAccessEnabled",
                schema: "Party",
                table: "PartyMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PartyUnitCompanyMapping",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyUnitCompanyMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyUnitCompanyMapping_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartyUnitCompanyMapping_PartyId",
                schema: "Party",
                table: "PartyUnitCompanyMapping",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyUnitCompanyMapping",
                schema: "Party");

            migrationBuilder.DropColumn(
                name: "IsPortalAccessEnabled",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
