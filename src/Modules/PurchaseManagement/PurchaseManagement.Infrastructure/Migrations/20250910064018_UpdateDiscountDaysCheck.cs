using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiscountDaysCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             // Drop old constraint
        migrationBuilder.DropCheckConstraint(
            name: "CK_PaymentTermMaster_DiscountDays",
            schema: "Purchase",
            table: "PaymentTermMaster");

        // Add the corrected rule:
        //  - No discount  -> DiscountDays must be NULL or 0
        //  - Has discount -> DiscountDays > 0
        migrationBuilder.AddCheckConstraint(
            name: "CK_PaymentTermMaster_DiscountDays",
            schema: "Purchase",
            table: "PaymentTermMaster",
            sql: "((ISNULL([DiscountPercent], 0) = 0 AND ISNULL([DiscountDays], 0) = 0) OR ([DiscountPercent] > 0 AND [DiscountDays] > 0))");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

               // Revert to the old behavior
        migrationBuilder.DropCheckConstraint(
            name: "CK_PaymentTermMaster_DiscountDays",
            schema: "Purchase",
            table: "PaymentTermMaster");

        migrationBuilder.AddCheckConstraint(
            name: "CK_PaymentTermMaster_DiscountDays",
            schema: "Purchase",
            table: "PaymentTermMaster",
            sql: "[DiscountPercent] IS NULL OR ([DiscountDays] IS NOT NULL AND [DiscountDays] > 0)");

        }
    }
}
