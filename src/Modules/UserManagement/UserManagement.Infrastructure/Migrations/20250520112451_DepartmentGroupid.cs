using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentGroupid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentGroupId",
                schema: "AppData",
                table: "Department",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentGroupId",
                schema: "AppData",
                table: "Department",
                column: "DepartmentGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_DepartmentGroup_DepartmentGroupId",
                schema: "AppData",
                table: "Department",
                column: "DepartmentGroupId",
                principalSchema: "AppData",
                principalTable: "DepartmentGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_DepartmentGroup_DepartmentGroupId",
                schema: "AppData",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_DepartmentGroupId",
                schema: "AppData",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "DepartmentGroupId",
                schema: "AppData",
                table: "Department");
        }
    }
}
