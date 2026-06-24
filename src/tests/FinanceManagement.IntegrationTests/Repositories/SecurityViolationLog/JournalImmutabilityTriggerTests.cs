using Dapper;
using Microsoft.Data.SqlClient;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.SecurityViolationLog;

namespace FinanceManagement.IntegrationTests.Repositories.SecurityViolationLog
{
    // US-GL01-10 — verifies the DB immutability triggers BLOCK + LOG mutations of posted journals.
    // Canonical trigger script: docs/JournalMaster/triggers/US-GL01-10_ImmutabilityTriggers.sql
    [Collection("DatabaseCollection")]
    public sealed class JournalImmutabilityTriggerTests
    {
        private readonly DbFixture _fixture;

        public JournalImmutabilityTriggerTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private const string CreateHeaderTrigger = @"
CREATE OR ALTER TRIGGER Finance.TR_JournalHeader_Immutable
ON Finance.JournalHeader
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @v TABLE (Id INT);   -- table variables survive ROLLBACK
    INSERT INTO @v (Id)
    SELECT d.Id FROM deleted d
        INNER JOIN Finance.MiscMaster dm  ON dm.Id  = d.StatusId
        INNER JOIN Finance.MiscTypeMaster dmt ON dmt.Id = dm.MiscTypeId AND dmt.MiscTypeCode = 'JOURNAL_STATUS'
        WHERE dm.Code IN ('POSTED', 'REVERSED')
          AND NOT EXISTS (
              SELECT 1 FROM inserted i
              INNER JOIN Finance.MiscMaster im  ON im.Id  = i.StatusId
              INNER JOIN Finance.MiscTypeMaster imt ON imt.Id = im.MiscTypeId AND imt.MiscTypeCode = 'JOURNAL_STATUS'
              WHERE i.Id = d.Id AND dm.Code = 'POSTED' AND im.Code = 'REVERSED'
          );

    IF EXISTS (SELECT 1 FROM @v)
    BEGIN
        DECLARE @action VARCHAR(10) = CASE WHEN EXISTS (SELECT 1 FROM inserted) THEN 'UPDATE' ELSE 'DELETE' END;
        ROLLBACK TRANSACTION;
        INSERT INTO Finance.SecurityViolationLog (TableName, JournalHeaderId, AttemptedAction, UserName, AttemptedAt, Channel)
        SELECT 'JournalHeader', Id, @action, SUSER_SNAME(), SYSDATETIMEOFFSET(), 'DB' FROM @v;
        RAISERROR ('Posted journals are immutable and cannot be updated or deleted.', 16, 1);
    END
END";

        private const string CreateDetailTrigger = @"
CREATE OR ALTER TRIGGER Finance.TR_JournalDetail_Immutable
ON Finance.JournalDetail
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @v TABLE (JournalHeaderId INT);
    INSERT INTO @v (JournalHeaderId)
    SELECT DISTINCT d.JournalHeaderId FROM deleted d
        INNER JOIN Finance.JournalHeader h ON h.Id = d.JournalHeaderId
        INNER JOIN Finance.MiscMaster m  ON m.Id  = h.StatusId
        INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = m.MiscTypeId
        WHERE mt.MiscTypeCode = 'JOURNAL_STATUS' AND m.Code IN ('POSTED', 'REVERSED');

    IF EXISTS (SELECT 1 FROM @v)
    BEGIN
        DECLARE @action VARCHAR(10) = CASE WHEN EXISTS (SELECT 1 FROM inserted) THEN 'UPDATE' ELSE 'DELETE' END;
        ROLLBACK TRANSACTION;
        INSERT INTO Finance.SecurityViolationLog (TableName, JournalHeaderId, AttemptedAction, UserName, AttemptedAt, Channel)
        SELECT 'JournalDetail', JournalHeaderId, @action, SUSER_SNAME(), SYSDATETIMEOFFSET(), 'DB' FROM @v;
        RAISERROR ('Lines of a posted journal are immutable.', 16, 1);
    END
END";

        [Fact]
        public async Task Triggers_Should_Block_And_Log_Mutations_Of_Posted_Journals()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Post a journal (no triggers yet) → status = Posted.
            int journalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                journalId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(journalId, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            try
            {
                await conn.ExecuteAsync(CreateHeaderTrigger);
                await conn.ExecuteAsync(CreateDetailTrigger);

                // AC-1: direct UPDATE on a posted header is blocked.
                var updateAttempt = async () => await conn.ExecuteAsync(
                    "UPDATE Finance.JournalHeader SET Narration = 'tampered' WHERE Id = @Id", new { Id = journalId });
                await updateAttempt.Should().ThrowAsync<SqlException>();

                // Direct DELETE on a posted line is blocked.
                var deleteAttempt = async () => await conn.ExecuteAsync(
                    "DELETE FROM Finance.JournalDetail WHERE JournalHeaderId = @Id", new { Id = journalId });
                await deleteAttempt.Should().ThrowAsync<SqlException>();

                // The posted data is unchanged (rolled back).
                var narration = await conn.ExecuteScalarAsync<string>(
                    "SELECT Narration FROM Finance.JournalHeader WHERE Id = @Id", new { Id = journalId });
                narration.Should().NotBe("tampered");
                (await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Finance.JournalDetail WHERE JournalHeaderId = @Id", new { Id = journalId }))
                    .Should().Be(2);

                // AC-3: both attempts are captured in the security violation log (persisted despite rollback).
                var repo = new SecurityViolationLogQueryRepository(new SqlConnection(_fixture.ConnectionString));
                var (logs, total) = await repo.GetAllAsync(1, 50, journalId);

                total.Should().BeGreaterThanOrEqualTo(2);
                logs.Should().Contain(l => l.TableName == "JournalHeader" && l.AttemptedAction == "UPDATE");
                logs.Should().Contain(l => l.TableName == "JournalDetail" && l.AttemptedAction == "DELETE");
                logs.Should().OnlyContain(l => l.Channel == "DB" && !string.IsNullOrEmpty(l.UserName));

                // Sanctioned exception: flipping a POSTED voucher to REVERSED (the reversal flow) is allowed.
                var reverseFlip = async () => await conn.ExecuteAsync(
                    "UPDATE Finance.JournalHeader SET StatusId = @Rev WHERE Id = @Id",
                    new { Rev = ids.StatusReversedId, Id = journalId });
                await reverseFlip.Should().NotThrowAsync();
                (await conn.ExecuteScalarAsync<int>(
                    "SELECT StatusId FROM Finance.JournalHeader WHERE Id = @Id", new { Id = journalId }))
                    .Should().Be(ids.StatusReversedId);

                // Lines of a REVERSED voucher remain immutable — soft-deleting them is blocked (reversal keeps
                // the original's lines intact for the audit trail).
                var softDeleteLines = async () => await conn.ExecuteAsync(
                    "UPDATE Finance.JournalDetail SET IsDeleted = 1 WHERE JournalHeaderId = @Id", new { Id = journalId });
                await softDeleteLines.Should().ThrowAsync<SqlException>();
                (await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Finance.JournalDetail WHERE JournalHeaderId = @Id AND IsDeleted = 1", new { Id = journalId }))
                    .Should().Be(0);
            }
            finally
            {
                // Drop the triggers so other tests' table cleanup (which deletes posted rows) is not blocked.
                await conn.ExecuteAsync("DROP TRIGGER IF EXISTS Finance.TR_JournalHeader_Immutable;");
                await conn.ExecuteAsync("DROP TRIGGER IF EXISTS Finance.TR_JournalDetail_Immutable;");
            }
        }
    }
}
