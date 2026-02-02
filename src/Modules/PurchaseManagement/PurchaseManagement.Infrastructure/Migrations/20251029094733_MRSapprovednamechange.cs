using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MRSapprovednamechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedByByName",
                schema: "Purchase",
                table: "MrsHeader",
                newName: "ApprovedByName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedByName",
                schema: "Purchase",
                table: "MrsHeader",
                newName: "ApprovedByByName");
        }
    }
}
