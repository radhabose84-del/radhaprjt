using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    public partial class budgetfinyear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔹 Safely drop old FK if exists
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_BudgetRequest_MiscMaster_MiscMasterId'
)
    ALTER TABLE [Budget].[BudgetRequest] 
    DROP CONSTRAINT [FK_BudgetRequest_MiscMaster_MiscMasterId];
");

            // 🔹 Safely drop old index if exists
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_BudgetRequest_MiscMasterId'
      AND object_id = OBJECT_ID('[Budget].[BudgetRequest]')
)
    DROP INDEX [IX_BudgetRequest_MiscMasterId] ON [Budget].[BudgetRequest];
");

            // 🔹 Handle MiscMasterId / RevisionNumber safely
            migrationBuilder.Sql(@"
IF COL_LENGTH('Budget.BudgetRequest', 'MiscMasterId') IS NOT NULL
BEGIN
    EXEC sp_rename N'[Budget].[BudgetRequest].[MiscMasterId]', N'RevisionNumber', 'COLUMN';
END
ELSE IF COL_LENGTH('Budget.BudgetRequest', 'RevisionNumber') IS NULL
BEGIN
    ALTER TABLE [Budget].[BudgetRequest]
    ADD [RevisionNumber] int NULL;
END
");

            // ❌ REMOVE this line completely (we replaced it with the SQL above)
            // migrationBuilder.RenameColumn(
            //     name: "MiscMasterId",
            //     schema: "Budget",
            //     table: "BudgetRequest",
            //     newName: "RevisionNumber");

            // 🔹 Keep the rest as generated
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.AddColumn<int>(
                name: "FinYearId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestById",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestMonthId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRequest_RequestById",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestById");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRequest_RequestMonthId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestMonthId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestById",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestById",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestMonthId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestMonthId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestTypeId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestById",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestMonthId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropIndex(
                name: "IX_BudgetRequest_RequestById",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropIndex(
                name: "IX_BudgetRequest_RequestMonthId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "FinYearId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "RequestById",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "RequestMonthId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.RenameColumn(
                name: "RevisionNumber",
                schema: "Budget",
                table: "BudgetRequest",
                newName: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRequest_MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "MiscMasterId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestTypeId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
