-- qa-clone-reset.sql : reset the ISOLATED QA clone to a clean baseline.
-- Deletes every row created by the QA user ('testsales') across the whole DB so
-- repeated UserManagement.QATests runs do not collide on accumulated rows.
-- Disables FK constraints, deletes, re-enables (no manual ordering needed).
-- SAFE: aborts unless connected to BannariERP_QATest.
-- USAGE: in SSMS pick BannariERP_QATest in the DB dropdown, then Execute.

SET NOCOUNT ON;

IF DB_NAME() <> N'BannariERP_QATest'
BEGIN
    RAISERROR(N'ABORTED: not connected to BannariERP_QATest. Select the clone and re-run.', 16, 1);
END
ELSE
BEGIN
    PRINT N'Resetting QA data in ' + DB_NAME() + N' ...';

    -- 1. Disable all FK constraints so deletes can run in any order.
    EXEC sys.sp_MSforeachtable @command1 = 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

    -- 2. Delete testsales-created rows from every table that has a CreatedByName column.
    DECLARE @sql nvarchar(max) = N'';
    SELECT @sql = @sql + N'DELETE FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
                 + N' WHERE CreatedByName = N''testsales'';' + NCHAR(13) + NCHAR(10)
    FROM sys.tables  t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE EXISTS (SELECT 1 FROM sys.columns c
                  WHERE c.object_id = t.object_id AND c.name = N'CreatedByName');

    EXEC sys.sp_executesql @sql;

    -- 3. Re-enable all FK constraints (no re-validation; the clone is throwaway).
    EXEC sys.sp_MSforeachtable @command1 = 'ALTER TABLE ? CHECK CONSTRAINT ALL';

    PRINT N'QA data reset complete.';
END
