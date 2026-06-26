-- =============================================================================
-- US-GL03-02 / AC#1 — Hard-Closed Period Posting Block
-- =============================================================================
-- Defence-in-depth trigger that hard-blocks INSERT/UPDATE on Finance.JournalLine
-- when the parent AccountingPeriod status is HARDCLOSED.
--
-- Post-refactor (2026-06-26): repointed from Finance.FinancialPeriodMaster (dropped) to
-- Finance.AccountingPeriod. The 3-state period status now lives on AccountingPeriod.StatusId
-- via MiscMaster rows under MiscType = 'FPS' (codes: OPEN / SOFTCLOSED / HARDCLOSED).
--
-- Conditional install:
--   * Finance.JournalLine ships in GL-01 FR-009 — until it exists this is a NoOp.
--   * Once JournalLine ships, the trigger activates automatically (idempotent re-create).
-- =============================================================================

IF OBJECT_ID('Finance.JournalLine', 'U') IS NOT NULL
BEGIN
    IF OBJECT_ID('Finance.trg_JournalLine_BlockHardClosedPeriod', 'TR') IS NOT NULL
        DROP TRIGGER [Finance].[trg_JournalLine_BlockHardClosedPeriod];

    EXEC('
        CREATE TRIGGER [Finance].[trg_JournalLine_BlockHardClosedPeriod]
        ON [Finance].[JournalLine]
        AFTER INSERT, UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;

            IF EXISTS (
                SELECT 1
                FROM   inserted i
                JOIN   [Finance].[AccountingPeriod] ap ON ap.Id = i.AccountingPeriodId
                JOIN   [Finance].[MiscMaster] mm        ON mm.Id = ap.StatusId AND mm.IsDeleted = 0
                JOIN   [Finance].[MiscTypeMaster] mtm   ON mtm.Id = mm.MiscTypeId AND mtm.IsDeleted = 0
                WHERE  mtm.MiscTypeCode = ''FPS''
                   AND mm.Code          = ''HARDCLOSED''
                   AND ap.IsDeleted     = 0
            )
            BEGIN
                ROLLBACK TRANSACTION;
                RAISERROR (
                    ''Cannot post to a HARDCLOSED fiscal period. Period status must be OPEN or SOFTCLOSED.'',
                    16, 1);
            END
        END
    ');
END;
