using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyGLcodecategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlCategoryId",
                schema: "Party",
                table: "PartyGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Glcode",
                schema: "Party",
                table: "PartyGroup",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyGroup_GlCategoryId",
                schema: "Party",
                table: "PartyGroup",
                column: "GlCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyGroup_MiscMaster_GlCategoryId",
                schema: "Party",
                table: "PartyGroup",
                column: "GlCategoryId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyGroup_MiscMaster_GlCategoryId",
                schema: "Party",
                table: "PartyGroup");

            migrationBuilder.DropIndex(
                name: "IX_PartyGroup_GlCategoryId",
                schema: "Party",
                table: "PartyGroup");

            migrationBuilder.DropColumn(
                name: "GlCategoryId",
                schema: "Party",
                table: "PartyGroup");

            migrationBuilder.DropColumn(
                name: "Glcode",
                schema: "Party",
                table: "PartyGroup");
        }
    }
}
