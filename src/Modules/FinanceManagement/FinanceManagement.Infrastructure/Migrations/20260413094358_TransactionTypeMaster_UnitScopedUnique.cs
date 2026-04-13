using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransactionTypeMaster_UnitScopedUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeMaster_ShortName",
                schema: "Finance",
                table: "TransactionTypeMaster");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeMaster_TypeName",
                schema: "Finance",
                table: "TransactionTypeMaster");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_UnitId_ShortName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                columns: new[] { "UnitId", "ShortName" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_UnitId_TypeName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                columns: new[] { "UnitId", "TypeName" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeMaster_UnitId_ShortName",
                schema: "Finance",
                table: "TransactionTypeMaster");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeMaster_UnitId_TypeName",
                schema: "Finance",
                table: "TransactionTypeMaster");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_ShortName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_TypeName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "TypeName",
                unique: true);
        }
    }
}
