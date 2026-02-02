using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class issuereturntblchangesfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocNo",
                schema: "Purchase",
                table: "IssueReturnDetail");

            migrationBuilder.DropColumn(
                name: "DocSno",
                schema: "Purchase",
                table: "IssueReturnDetail");

            migrationBuilder.AlterColumn<int>(
                name: "TargetId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StorageTypeId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TargetId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StorageTypeId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocNo",
                schema: "Purchase",
                table: "IssueReturnDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocSno",
                schema: "Purchase",
                table: "IssueReturnDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
