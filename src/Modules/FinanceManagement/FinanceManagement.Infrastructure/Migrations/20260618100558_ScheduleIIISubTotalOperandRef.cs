using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIISubTotalOperandRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "OperandSubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotalFormula_OperandSubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "OperandSubTotalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIISubTotalFormula_ScheduleIIISubTotal_OperandSubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "OperandSubTotalId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIISubTotal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIISubTotalFormula_ScheduleIIISubTotal_OperandSubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotalFormula_OperandSubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.DropColumn(
                name: "OperandSubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula");

            migrationBuilder.AlterColumn<int>(
                name: "SubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
