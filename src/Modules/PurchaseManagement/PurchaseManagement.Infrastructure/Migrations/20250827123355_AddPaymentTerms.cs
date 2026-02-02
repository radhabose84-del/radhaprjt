using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTermMaster",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BaselineTypeId = table.Column<int>(type: "int", nullable: false),
                    CreditDays = table.Column<int>(type: "int", nullable: false),
                    AdvancePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    BalancePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, computedColumnSql: "CONVERT(decimal(5,2), 100.00 - ISNULL([AdvancePercent], 0))", stored: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DiscountDays = table.Column<int>(type: "int", nullable: true),
                    GraceDays = table.Column<int>(type: "int", nullable: true),
                    ApplicableForPortal = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTermMaster", x => x.Id);
                    table.CheckConstraint("CK_PaymentTermMaster_AdvancePercent", "[AdvancePercent] IS NULL OR ([AdvancePercent] >= 0 AND [AdvancePercent] <= 100)");
                    table.CheckConstraint("CK_PaymentTermMaster_CreditDays", "[CreditDays] >= 0");
                    table.CheckConstraint("CK_PaymentTermMaster_DiscountDays", "[DiscountPercent] IS NULL OR ([DiscountDays] IS NOT NULL AND [DiscountDays] > 0)");
                    table.CheckConstraint("CK_PaymentTermMaster_DiscountPercent", "[DiscountPercent] IS NULL OR ([DiscountPercent] >= 0 AND [DiscountPercent] <= 100)");
                    table.CheckConstraint("CK_PaymentTermMaster_GraceDays", "[GraceDays] IS NULL OR [GraceDays] >= 0");
                    table.ForeignKey(
                        name: "FK_PaymentTermMaster_MiscMaster_BaselineTypeId",
                        column: x => x.BaselineTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTermInstallment",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    SeqNo = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DueDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTermInstallment", x => x.Id);
                    table.CheckConstraint("CK_PaymentTermInstallment_DueDays", "[DueDays] >= 0");
                    table.CheckConstraint("CK_PaymentTermInstallment_Percent", "[Percent] >= 0 AND [Percent] <= 100");
                    table.CheckConstraint("CK_PaymentTermInstallment_SeqNo", "[SeqNo] >= 1");
                    table.ForeignKey(
                        name: "FK_PaymentTermInstallment_PaymentTermMaster_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalSchema: "Purchase",
                        principalTable: "PaymentTermMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTermInstallment_PaymentTermId_SeqNo",
                schema: "Purchase",
                table: "PaymentTermInstallment",
                columns: new[] { "PaymentTermId", "SeqNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTermMaster_BaselineTypeId",
                schema: "Purchase",
                table: "PaymentTermMaster",
                column: "BaselineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTermMaster_Code",
                schema: "Purchase",
                table: "PaymentTermMaster",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTermInstallment",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PaymentTermMaster",
                schema: "Purchase");
        }
    }
}
