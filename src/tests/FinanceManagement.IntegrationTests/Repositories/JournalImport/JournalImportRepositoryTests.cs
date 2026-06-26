using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalImport;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.JournalImport
{
    [Collection("DatabaseCollection")]
    public sealed class JournalImportRepositoryTests
    {
        private readonly DbFixture _fixture;

        public JournalImportRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private JournalImportCommandRepository CreateCommandRepo(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);
        private JournalImportQueryRepository CreateQueryRepo() => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // Adds IMPORT to the (already-seeded) JOURNAL_SOURCE type + an IMPORT_BATCH_STATUS type with COMMITTED/FAILED.
        private async Task<(int importSourceId, int committedId, int failedId)> SeedImportLookupsAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var sourceType = await ctx.MiscTypeMaster.FirstAsync(t => t.MiscTypeCode == "JOURNAL_SOURCE");
            var import = new MiscMaster { MiscTypeId = sourceType.Id, Code = "IMPORT", Description = "Excel Import", SortOrder = 9, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.Add(import);

            var batchType = new MiscTypeMaster { MiscTypeCode = "IMPORT_BATCH_STATUS", Description = "Import Batch Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.Add(batchType);
            await ctx.SaveChangesAsync();

            var committed = new MiscMaster { MiscTypeId = batchType.Id, Code = "COMMITTED", Description = "Committed", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var failed = new MiscMaster { MiscTypeId = batchType.Id, Code = "FAILED", Description = "Failed", SortOrder = 2, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.AddRange(committed, failed);
            await ctx.SaveChangesAsync();

            return (import.Id, committed.Id, failed.Id);
        }

        private static JournalImportBatch BuildBatch(int statusId, int sourceId, int total, int valid, int error) =>
            new()
            {
                FileName = "June_Accruals.xlsx", TotalRows = total, ValidRows = valid, ErrorRows = error,
                StatusId = statusId, SourceId = sourceId, ImportedBy = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task CommitAsync_Should_Persist_Batch_And_Draft_Tagged_Import()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var (importSourceId, committedId, _) = await SeedImportLookupsAsync();

            var draft = JournalTestSeed.BuildDraftJournal(ids);
            draft.SourceId = importSourceId;

            var fyNames = new Dictionary<int, string> { [draft.FinancialYearId] = "2026-27" };

            int batchId;
            List<int> journalIds;
            await using (var ctx = _fixture.CreateFreshDbContext())
                (batchId, journalIds) = await CreateCommandRepo(ctx).CommitAsync(
                    BuildBatch(committedId, importSourceId, 2, 2, 0),
                    new List<JournalHeader> { draft },
                    fyNames,
                    CancellationToken.None);

            batchId.Should().BeGreaterThan(0);
            journalIds.Should().ContainSingle();

            await using var verify = _fixture.CreateFreshDbContext();
            var savedJournal = await verify.JournalHeader.FirstAsync(h => h.Id == journalIds[0]);
            savedJournal.ImportBatchId.Should().Be(batchId);
            savedJournal.SourceId.Should().Be(importSourceId);
            (await verify.JournalDetail.CountAsync(d => d.JournalHeaderId == journalIds[0])).Should().Be(2);

            // GetBatchByIdAsync returns the created journal header + its line items.
            var dto = await CreateQueryRepo().GetBatchByIdAsync(batchId);
            dto.Should().NotBeNull();
            dto!.Journals.Should().ContainSingle();
            var jrnl = dto.Journals[0];
            jrnl.Id.Should().Be(journalIds[0]);
            jrnl.VoucherNo.Should().NotBeNull();    // voucher number allocated at import (like manual create)
            jrnl.IsPosted.Should().BeFalse();        // still a DRAFT until posted
            jrnl.Lines.Should().HaveCount(2);
            jrnl.Lines.Should().OnlyContain(l => l.JournalHeaderId == journalIds[0]);
        }

        [Fact]
        public async Task SaveFailedBatchAsync_Should_Persist_Batch_And_Errors_No_Journal()
        {
            await ClearTableAsync();
            await JournalTestSeed.SeedGraphAsync(_fixture);
            var (importSourceId, _, failedId) = await SeedImportLookupsAsync();

            var errors = new List<JournalImportError>
            {
                new() { RowNo = 47, ColumnName = "gl_account", Message = "GL account does not exist." },
                new() { RowNo = 88, ColumnName = "dr/cr", Message = "Voucher group 2 is out of balance." }
            };

            int batchId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                batchId = await CreateCommandRepo(ctx).SaveFailedBatchAsync(BuildBatch(failedId, importSourceId, 2, 0, 2), errors, CancellationToken.None);

            await using var verify = _fixture.CreateFreshDbContext();
            (await verify.JournalImportError.CountAsync(e => e.ImportBatchId == batchId)).Should().Be(2);
            (await verify.JournalHeader.CountAsync()).Should().Be(0);   // no partial commit

            var dto = await CreateQueryRepo().GetBatchByIdAsync(batchId);
            dto.Should().NotBeNull();
            dto!.Errors.Should().HaveCount(2);
            dto.StatusName.Should().Be("Failed");
        }

        [Fact]
        public async Task QueryHelpers_Should_Resolve_Seeded_Data()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var (importSourceId, committedId, _) = await SeedImportLookupsAsync();
            var repo = CreateQueryRepo();

            (await repo.GetSourceIdAsync("IMPORT")).Should().Be(importSourceId);
            (await repo.GetBatchStatusIdAsync("COMMITTED")).Should().Be(committedId);
            (await repo.GetExistingGlAccountIdsAsync(new[] { ids.GlAccountDrId, 999999 }, 1)).Should().Contain(ids.GlAccountDrId);
            (await repo.GetExistingCurrencyIdsAsync(new[] { ids.CurrencyId })).Should().Contain(ids.CurrencyId);

            var period = await repo.GetOpenPeriodByDateAsync(1, new DateOnly(2026, 6, 15));
            period.Should().NotBeNull();
            period!.Value.PeriodId.Should().Be(ids.AccountingPeriodId);
        }
    }
}
