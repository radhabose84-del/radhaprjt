using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VoucherTypeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTypeAccountType_AccountTypeMaster_AccountTypeId",
                table: "VoucherTypeAccountType");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTypeAccountType_VoucherTypeMaster_VoucherTypeId",
                table: "VoucherTypeAccountType");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTypeNumberSeries_VoucherTypeMaster_VoucherTypeId",
                table: "VoucherTypeNumberSeries");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTypeMaster_CompanyId_SeriesPrefix",
                schema: "Finance",
                table: "VoucherTypeMaster");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTypeMaster_CompanyId_VoucherTypeCode",
                schema: "Finance",
                table: "VoucherTypeMaster");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTypeAccountType_VoucherTypeId",
                table: "VoucherTypeAccountType");

            migrationBuilder.DropColumn(
                name: "SeriesPrefix",
                schema: "Finance",
                table: "VoucherTypeMaster");

            migrationBuilder.RenameTable(
                name: "VoucherTypeNumberSeries",
                newName: "VoucherTypeNumberSeries",
                newSchema: "Finance");

            migrationBuilder.RenameTable(
                name: "VoucherTypeAccountType",
                newName: "VoucherTypeAccountType",
                newSchema: "Finance");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                schema: "Finance",
                table: "VoucherTypeMaster",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LastUsedNumber",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeMaster_CompanyId_VoucherTypeCode",
                schema: "Finance",
                table: "VoucherTypeMaster",
                columns: new[] { "CompanyId", "VoucherTypeCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeNumberSeries_VoucherTypeId_FinancialYearId",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                columns: new[] { "VoucherTypeId", "FinancialYearId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeAccountType_VoucherTypeId_AccountTypeId",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                columns: new[] { "VoucherTypeId", "AccountTypeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTypeAccountType_AccountTypeMaster_AccountTypeId",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                column: "AccountTypeId",
                principalSchema: "Finance",
                principalTable: "AccountTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTypeAccountType_VoucherTypeMaster_VoucherTypeId",
                schema: "Finance",
                table: "VoucherTypeAccountType",
                column: "VoucherTypeId",
                principalSchema: "Finance",
                principalTable: "VoucherTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTypeNumberSeries_VoucherTypeMaster_VoucherTypeId",
                schema: "Finance",
                table: "VoucherTypeNumberSeries",
                column: "VoucherTypeId",
                principalSchema: "Finance",
                principalTable: "VoucherTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTypeAccountType_AccountTypeMaster_AccountTypeId",
                schema: "Finance",
                table: "VoucherTypeAccountType");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTypeAccountType_VoucherTypeMaster_VoucherTypeId",
                schema: "Finance",
                table: "VoucherTypeAccountType");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTypeNumberSeries_VoucherTypeMaster_VoucherTypeId",
                schema: "Finance",
                table: "VoucherTypeNumberSeries");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTypeMaster_CompanyId_VoucherTypeCode",
                schema: "Finance",
                table: "VoucherTypeMaster");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTypeNumberSeries_VoucherTypeId_FinancialYearId",
                schema: "Finance",
                table: "VoucherTypeNumberSeries");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTypeAccountType_VoucherTypeId_AccountTypeId",
                schema: "Finance",
                table: "VoucherTypeAccountType");

            migrationBuilder.RenameTable(
                name: "VoucherTypeNumberSeries",
                schema: "Finance",
                newName: "VoucherTypeNumberSeries");

            migrationBuilder.RenameTable(
                name: "VoucherTypeAccountType",
                schema: "Finance",
                newName: "VoucherTypeAccountType");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                schema: "Finance",
                table: "VoucherTypeMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SeriesPrefix",
                schema: "Finance",
                table: "VoucherTypeMaster",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                table: "VoucherTypeNumberSeries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                table: "VoucherTypeNumberSeries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LastUsedNumber",
                table: "VoucherTypeNumberSeries",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "VoucherTypeNumberSeries",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "VoucherTypeNumberSeries",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "VoucherTypeNumberSeries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "VoucherTypeNumberSeries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                table: "VoucherTypeAccountType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                table: "VoucherTypeAccountType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "VoucherTypeAccountType",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "VoucherTypeAccountType",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "VoucherTypeAccountType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "VoucherTypeAccountType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeMaster_CompanyId_SeriesPrefix",
                schema: "Finance",
                table: "VoucherTypeMaster",
                columns: new[] { "CompanyId", "SeriesPrefix" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeMaster_CompanyId_VoucherTypeCode",
                schema: "Finance",
                table: "VoucherTypeMaster",
                columns: new[] { "CompanyId", "VoucherTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeAccountType_VoucherTypeId",
                table: "VoucherTypeAccountType",
                column: "VoucherTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTypeAccountType_AccountTypeMaster_AccountTypeId",
                table: "VoucherTypeAccountType",
                column: "AccountTypeId",
                principalSchema: "Finance",
                principalTable: "AccountTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTypeAccountType_VoucherTypeMaster_VoucherTypeId",
                table: "VoucherTypeAccountType",
                column: "VoucherTypeId",
                principalSchema: "Finance",
                principalTable: "VoucherTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTypeNumberSeries_VoucherTypeMaster_VoucherTypeId",
                table: "VoucherTypeNumberSeries",
                column: "VoucherTypeId",
                principalSchema: "Finance",
                principalTable: "VoucherTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
