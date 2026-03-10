using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentSequence_RenameTypeIdToTransactionTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSequence_TransactionTypeMaster_TypeId",
                schema: "Finance",
                table: "DocumentSequence");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                schema: "Finance",
                table: "DocumentSequence",
                newName: "TransactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentSequence_TypeId_FinancialYearId_DocNo",
                schema: "Finance",
                table: "DocumentSequence",
                newName: "IX_DocumentSequence_TransactionTypeId_FinancialYearId_DocNo");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentSequence_TypeId",
                schema: "Finance",
                table: "DocumentSequence",
                newName: "IX_DocumentSequence_TransactionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSequence_TransactionTypeMaster_TransactionTypeId",
                schema: "Finance",
                table: "DocumentSequence",
                column: "TransactionTypeId",
                principalSchema: "Finance",
                principalTable: "TransactionTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSequence_TransactionTypeMaster_TransactionTypeId",
                schema: "Finance",
                table: "DocumentSequence");

            migrationBuilder.RenameColumn(
                name: "TransactionTypeId",
                schema: "Finance",
                table: "DocumentSequence",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentSequence_TransactionTypeId_FinancialYearId_DocNo",
                schema: "Finance",
                table: "DocumentSequence",
                newName: "IX_DocumentSequence_TypeId_FinancialYearId_DocNo");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentSequence_TransactionTypeId",
                schema: "Finance",
                table: "DocumentSequence",
                newName: "IX_DocumentSequence_TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSequence_TransactionTypeMaster_TypeId",
                schema: "Finance",
                table: "DocumentSequence",
                column: "TypeId",
                principalSchema: "Finance",
                principalTable: "TransactionTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
