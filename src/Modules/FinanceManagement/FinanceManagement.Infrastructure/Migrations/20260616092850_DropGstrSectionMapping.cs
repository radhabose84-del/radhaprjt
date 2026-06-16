using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropGstrSectionMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GstrSectionMapping",
                schema: "Finance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GstrSectionMapping",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountRangeFrom = table.Column<string>(type: "varchar(20)", nullable: false),
                    AccountRangeTo = table.Column<string>(type: "varchar(20)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    GstrType = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    SectionCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(150)", nullable: false),
                    TolerancePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GstrSectionMapping", x => x.Id);
                    table.CheckConstraint("CK_GSM_Tolerance", "[TolerancePercent] IS NULL OR [TolerancePercent] >= 0");
                    table.CheckConstraint("CK_GSM_Type", "[GstrType] IN ('GSTR1','GSTR3B')");
                });

            migrationBuilder.CreateIndex(
                name: "UX_GstrSectionMapping_Section",
                schema: "Finance",
                table: "GstrSectionMapping",
                columns: new[] { "CompanyId", "GstrType", "SectionCode" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
