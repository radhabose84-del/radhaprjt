using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIISubTotalRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotal_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotal_SubTotalTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "OperandRefId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "IsSystemDefined",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "SubTotalTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.AlterColumn<int>(
                name: "SubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SectionItemId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormulaName",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "varchar(120)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotalFormula_SectionItemId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "SectionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotal_FormulaName",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                column: "FormulaName");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIISubTotalFormula_ScheduleIIISectionItem_SectionItemId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "SectionItemId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIISectionItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIISubTotalFormula_ScheduleIIISectionItem_SectionItemId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotalFormula_SectionItemId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotal_FormulaName",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "SectionItemId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.DropColumn(
                name: "FormulaName",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.AlterColumn<int>(
                name: "SubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperandRefId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemDefined",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "SubTotalTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotal_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                columns: new[] { "CompanyId", "DivisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotal_SubTotalTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                column: "SubTotalTypeId");
        }
    }
}
