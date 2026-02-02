using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMiscMasterTableAlterAddMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeMasterId",
                table: "MiscMaster");

            migrationBuilder.DropIndex(
                name: "IX_MiscMaster_MiscTypeMasterId",
                table: "MiscMaster");

            migrationBuilder.DropColumn(
                name: "MiscTypeMasterId",
                table: "MiscMaster");

            migrationBuilder.RenameTable(
                name: "MiscMaster",
                newName: "MiscMaster",
                newSchema: "Purchase");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                schema: "Purchase",
                table: "MiscMaster",
                newName: "sortOrder");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "Purchase",
                table: "MiscMaster",
                newName: "description");

            migrationBuilder.AlterColumn<int>(
                name: "sortOrder",
                schema: "Purchase",
                table: "MiscMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "Purchase",
                table: "MiscMaster",
                type: "varchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "Purchase",
                table: "MiscMaster",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "Purchase",
                table: "MiscMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Purchase",
                table: "MiscMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "Purchase",
                table: "MiscMaster",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Purchase",
                table: "MiscMaster",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "Purchase",
                table: "MiscMaster",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Purchase",
                table: "MiscMaster",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiscMaster_MiscTypeId",
                schema: "Purchase",
                table: "MiscMaster",
                column: "MiscTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeId",
                schema: "Purchase",
                table: "MiscMaster",
                column: "MiscTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeId",
                schema: "Purchase",
                table: "MiscMaster");

            migrationBuilder.DropIndex(
                name: "IX_MiscMaster_MiscTypeId",
                schema: "Purchase",
                table: "MiscMaster");

            migrationBuilder.RenameTable(
                name: "MiscMaster",
                schema: "Purchase",
                newName: "MiscMaster");

            migrationBuilder.RenameColumn(
                name: "sortOrder",
                table: "MiscMaster",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "MiscMaster",
                newName: "Description");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "MiscMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MiscMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                table: "MiscMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                table: "MiscMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "MiscMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "MiscMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "MiscMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "MiscMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "MiscMaster",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AddColumn<int>(
                name: "MiscTypeMasterId",
                table: "MiscMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiscMaster_MiscTypeMasterId",
                table: "MiscMaster",
                column: "MiscTypeMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MiscMaster_MiscTypeMaster_MiscTypeMasterId",
                table: "MiscMaster",
                column: "MiscTypeMasterId",
                principalSchema: "Purchase",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id");
        }
    }
}
