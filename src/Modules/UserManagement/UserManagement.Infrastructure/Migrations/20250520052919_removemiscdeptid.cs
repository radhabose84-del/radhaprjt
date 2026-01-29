using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removemiscdeptid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentGroup_MiscMaster_MiscDeptId",
                schema: "AppData",
                table: "DepartmentGroup");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentGroup_MiscDeptId",
                schema: "AppData",
                table: "DepartmentGroup");

            migrationBuilder.DropColumn(
                name: "MiscDeptId",
                schema: "AppData",
                table: "DepartmentGroup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MiscDeptId",
                schema: "AppData",
                table: "DepartmentGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentGroup_MiscDeptId",
                schema: "AppData",
                table: "DepartmentGroup",
                column: "MiscDeptId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentGroup_MiscMaster_MiscDeptId",
                schema: "AppData",
                table: "DepartmentGroup",
                column: "MiscDeptId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
