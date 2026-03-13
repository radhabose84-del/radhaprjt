using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Finance.TransactionTypeMaster — already exists (created by SalesManagement migrations)
            // Finance.DocumentSequence      — already exists (created by SalesManagement migrations)
            // Finance.EInvoiceHeader        — already exists (moved from Sales schema via ALTER SCHEMA TRANSFER)
            // Finance.EInvoiceDetail        — already exists (moved from Sales schema via ALTER SCHEMA TRANSFER)
            //
            // All four Finance tables were present in the database before this migration ran.
            // This Up() is intentionally empty — it only registers the migration in __EFMigrationsHistory.
            //
            // To recreate from scratch on a new database, run the following SQL first:
            //   ALTER SCHEMA Finance TRANSFER Sales.EInvoiceDetail;  (drop FK first)
            //   ALTER SCHEMA Finance TRANSFER Sales.EInvoiceHeader;
            // OR apply the full DDL from the Down() rollback script manually.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EInvoiceDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "EInvoiceHeader",
                schema: "Finance");

            // Finance.TransactionTypeMaster and Finance.DocumentSequence are intentionally
            // NOT dropped here — they are owned by SalesManagement migration history.
        }
    }
}
