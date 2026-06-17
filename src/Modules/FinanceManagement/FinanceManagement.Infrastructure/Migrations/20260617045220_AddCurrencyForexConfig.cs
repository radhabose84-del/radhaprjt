using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyForexConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyForexConfig",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CurrencyTypeCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    CurrencyTypeName = table.Column<string>(type: "varchar(100)", nullable: false),
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
                    table.PrimaryKey("PK_CurrencyForexConfig", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_CurrencyTypeId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "CurrencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyForexConfig_CompanyId",
                schema: "Finance",
                table: "CurrencyForexConfig",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyForexConfig_CompanyId_CurrencyTypeCode",
                schema: "Finance",
                table: "CurrencyForexConfig",
                columns: new[] { "CompanyId", "CurrencyTypeCode" },
                unique: true);

            // US-GL02-12: seed CurrencyForexConfig per company that owns GL accounts,
            // then repoint existing GlAccountMaster.CurrencyTypeId to a valid row BEFORE the FK
            // is added (existing GL rows can't be deleted — they're referenced by TaxAccountLinkage).
            migrationBuilder.Sql(@"
                INSERT INTO Finance.CurrencyForexConfig
                    (CompanyId, CurrencyTypeCode, CurrencyTypeName, IsActive, IsDeleted,
                     CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                SELECT c.CompanyId, v.Code, v.Name, 1, 0,
                       1, 'Vishal', '127.0.0.1', SYSDATETIMEOFFSET()
                FROM (SELECT DISTINCT CompanyId FROM Finance.GlAccountMaster) c
                CROSS JOIN (VALUES ('INRONLY','INR-only'),('FOREX','Forex'),('MULTICUR','Multi-currency')) AS v(Code, Name)
                WHERE NOT EXISTS (
                    SELECT 1 FROM Finance.CurrencyForexConfig x
                    WHERE x.CompanyId = c.CompanyId AND x.CurrencyTypeCode = v.Code);

                UPDATE am
                SET am.CurrencyTypeId = cfc.Id
                FROM Finance.GlAccountMaster am
                INNER JOIN Finance.CurrencyForexConfig cfc
                    ON cfc.CompanyId = am.CompanyId AND cfc.CurrencyTypeCode = 'INRONLY'
                WHERE am.CurrencyTypeId NOT IN (SELECT Id FROM Finance.CurrencyForexConfig);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_GlAccountMaster_CurrencyForexConfig_CurrencyTypeId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "CurrencyTypeId",
                principalSchema: "Finance",
                principalTable: "CurrencyForexConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlAccountMaster_CurrencyForexConfig_CurrencyTypeId",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropTable(
                name: "CurrencyForexConfig",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_GlAccountMaster_CurrencyTypeId",
                schema: "Finance",
                table: "GlAccountMaster");
        }
    }
}
