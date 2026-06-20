/* ============================================================================
   US-GL02-FR-008a  —  COA Freeze enforcement triggers  (DBA deliverable, AC1 + AC4)

   Enforcement is at the DATABASE level, NOT application code: while a company's
   Finance.CoaFreezeState.IsFrozen = 1, ANY structural write (INSERT / UPDATE / DELETE)
   to the chart of accounts is rejected and rolled back — regardless of source
   (UI, API or raw SQL). The app's "DB Trigger: ACTIVE" badge verifies these exist
   and are enabled (sys.triggers, is_disabled = 0).

   PRE-REQ: run the EF migration that creates Finance.CoaFreezeState FIRST.
   APPLY:   DBA reviews + executes this script against the target database.
   ============================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---- GlAccountMaster: blocks new accounts + code/field changes while frozen ---- */
CREATE OR ALTER TRIGGER Finance.trg_GlAccountMaster_CoaFreeze
ON Finance.GlAccountMaster
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM Finance.CoaFreezeState fs
        WHERE fs.IsFrozen = 1
          AND fs.IsDeleted = 0
          AND fs.CompanyId IN (SELECT CompanyId FROM inserted
                               UNION
                               SELECT CompanyId FROM deleted)
    )
    BEGIN
        RAISERROR ('COA_FREEZE_VIOLATION', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

/* ---- AccountGroup: blocks new groups, restructure (parent change) and
        FS-remap (ScheduleIII line change) while frozen ---- */
CREATE OR ALTER TRIGGER Finance.trg_AccountGroup_CoaFreeze
ON Finance.AccountGroup
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM Finance.CoaFreezeState fs
        WHERE fs.IsFrozen = 1
          AND fs.IsDeleted = 0
          AND fs.CompanyId IN (SELECT CompanyId FROM inserted
                               UNION
                               SELECT CompanyId FROM deleted)
    )
    BEGIN
        RAISERROR ('COA_FREEZE_VIOLATION', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

/* ---- Verify (feeds the "DB Trigger: ACTIVE" badge) ----
SELECT name, is_disabled
FROM sys.triggers
WHERE name IN ('trg_GlAccountMaster_CoaFreeze', 'trg_AccountGroup_CoaFreeze');
*/

/* ---- Rollback (if ever needed) ----
DROP TRIGGER IF EXISTS Finance.trg_GlAccountMaster_CoaFreeze;
DROP TRIGGER IF EXISTS Finance.trg_AccountGroup_CoaFreeze;
*/
