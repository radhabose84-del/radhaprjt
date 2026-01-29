using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removemiscmaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Generator_MiscMaster_GensetStatusId",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.DropIndex(
                name: "IX_Generator_GensetStatusId",
                schema: "Maintenance",
                table: "Generator");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Generator_GensetStatusId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Generator_MiscMaster_GensetStatusId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
