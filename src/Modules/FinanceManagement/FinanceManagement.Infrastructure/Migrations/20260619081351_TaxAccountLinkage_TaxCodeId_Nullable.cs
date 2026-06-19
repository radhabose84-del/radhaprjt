using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaxAccountLinkage_TaxCodeId_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "VoucherTypeMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    VoucherTypeCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    VoucherTypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SeriesPrefix = table.Column<string>(type: "varchar(10)", nullable: false),
                    NumberPadding = table.Column<int>(type: "int", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherTypeMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoucherTypeAccountType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    AccountTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherTypeAccountType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherTypeAccountType_AccountTypeMaster_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalSchema: "Finance",
                        principalTable: "AccountTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherTypeAccountType_VoucherTypeMaster_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalSchema: "Finance",
                        principalTable: "VoucherTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoucherTypeNumberSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    LastUsedNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherTypeNumberSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherTypeNumberSeries_VoucherTypeMaster_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalSchema: "Finance",
                        principalTable: "VoucherTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeAccountType_AccountTypeId",
                table: "VoucherTypeAccountType",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeAccountType_VoucherTypeId",
                table: "VoucherTypeAccountType",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTypeMaster_CompanyId",
                schema: "Finance",
                table: "VoucherTypeMaster",
                column: "CompanyId");

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
                name: "IX_VoucherTypeNumberSeries_VoucherTypeId",
                table: "VoucherTypeNumberSeries",
                column: "VoucherTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoucherTypeAccountType");

            migrationBuilder.DropTable(
                name: "VoucherTypeNumberSeries");

            migrationBuilder.DropTable(
                name: "VoucherTypeMaster",
                schema: "Finance");

            migrationBuilder.AlterColumn<int>(
                name: "TaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
