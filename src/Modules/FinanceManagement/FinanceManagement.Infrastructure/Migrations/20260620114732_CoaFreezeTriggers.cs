using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CoaFreezeTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // US-GL02-FR-008a — COA freeze enforcement triggers (AC1/AC4). While a company's
            // Finance.CoaFreezeState.IsFrozen = 1, any structural write to the chart of accounts
            // (INSERT/UPDATE/DELETE via UI, API or raw SQL) is rejected and rolled back.
            // Each CREATE TRIGGER must be its own batch, so two separate Sql() calls (no GO).
            migrationBuilder.Sql(@"
                CREATE OR ALTER TRIGGER Finance.trg_GlAccountMaster_CoaFreeze
                ON Finance.GlAccountMaster
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM Finance.CoaFreezeState fs
                               WHERE fs.IsFrozen = 1 AND fs.IsDeleted = 0
                                 AND fs.CompanyId IN (SELECT CompanyId FROM inserted
                                                      UNION SELECT CompanyId FROM deleted))
                    BEGIN
                        RAISERROR ('COA_FREEZE_VIOLATION', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END
                END");

            migrationBuilder.Sql(@"
                CREATE OR ALTER TRIGGER Finance.trg_AccountGroup_CoaFreeze
                ON Finance.AccountGroup
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM Finance.CoaFreezeState fs
                               WHERE fs.IsFrozen = 1 AND fs.IsDeleted = 0
                                 AND fs.CompanyId IN (SELECT CompanyId FROM inserted
                                                      UNION SELECT CompanyId FROM deleted))
                    BEGIN
                        RAISERROR ('COA_FREEZE_VIOLATION', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Finance.trg_GlAccountMaster_CoaFreeze;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS Finance.trg_AccountGroup_CoaFreeze;");
        }
    }
}
