using Microsoft.EntityFrameworkCore;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.Journal
{
    [Collection("DatabaseCollection")]
    public sealed class JournalCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public JournalCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private JournalCommandRepository CreateRepository(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Lines()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            int newId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                newId = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            newId.Should().BeGreaterThan(0);

            await using var verify = _fixture.CreateFreshDbContext();
            var lines = await verify.JournalDetail.Where(d => d.JournalHeaderId == newId).ToListAsync();
            lines.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Lines()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = JournalTestSeed.BuildDraftJournal(ids, amount: 2500m);
                entity.Id = id;
                await CreateRepository(ctx).UpdateAsync(entity);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var header = await verify.JournalHeader.FirstAsync(h => h.Id == id);
            var lines = await verify.JournalDetail.Where(d => d.JournalHeaderId == id).ToListAsync();
            header.TotalDr.Should().Be(2500m);
            lines.Should().HaveCount(2);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
                (await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None)).Should().BeTrue();

            await using var verify = _fixture.CreateFreshDbContext();
            var deleted = await verify.JournalHeader.IgnoreQueryFilters().FirstAsync(h => h.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        // --- POSTING (US-GL01-09) ---

        [Fact]
        public async Task PostAsync_Should_Assign_VoucherNo_And_Set_Posted()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            var now = DateTimeOffset.UtcNow;
            FinanceManagement.Application.JournalMaster.Dto.PostJournalResultDto? result;
            await using (var ctx = _fixture.CreateFreshDbContext())
                result = await CreateRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, now, CancellationToken.None);

            result.Should().NotBeNull();
            result!.VoucherNo.Should().Be("JV/2026-27/0001");

            await using var verify = _fixture.CreateFreshDbContext();
            var header = await verify.JournalHeader.FirstAsync(h => h.Id == id);
            header.VoucherNo.Should().Be("JV/2026-27/0001");
            header.StatusId.Should().Be(ids.StatusPostedId);
            header.PostingDate.Should().NotBeNull();
        }

        [Fact]
        public async Task PostAsync_Should_Update_LedgerBalance()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids, amount: 1000m));

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            await using var verify = _fixture.CreateFreshDbContext();
            var balances = await verify.LedgerBalance.ToListAsync();
            balances.Should().HaveCount(2);

            var drBal = balances.First(b => b.GlAccountId == ids.GlAccountDrId);
            drBal.DrTotal.Should().Be(1000m);
            drBal.Balance.Should().Be(1000m);
            drBal.CostCentreId.Should().Be(ids.CostCentreId);

            var crBal = balances.First(b => b.GlAccountId == ids.GlAccountCrId);
            crBal.CrTotal.Should().Be(1000m);
            crBal.Balance.Should().Be(-1000m);
            crBal.CostCentreId.Should().BeNull();
        }

        [Fact]
        public async Task PostAsync_Should_Increment_NumberSeries()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            await using var verify = _fixture.CreateFreshDbContext();
            var series = await verify.VoucherTypeNumberSeries
                .FirstAsync(s => s.VoucherTypeId == ids.VoucherTypeId && s.FinancialYearId == ids.FinancialYearId);
            series.LastUsedNumber.Should().Be(1);
        }

        [Fact]
        public async Task PostAsync_ConcurrentPosts_AssignDistinctSequentialNumbers()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Create N drafts up front.
            const int n = 5;
            var draftIds = new List<int>();
            for (var i = 0; i < n; i++)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                draftIds.Add(await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids)));
            }

            // Post them all at once, each on its own context/connection (real concurrency).
            var now = DateTimeOffset.UtcNow;
            var results = await Task.WhenAll(draftIds.Select(id => Task.Run(async () =>
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                return await CreateRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, now, CancellationToken.None);
            })));

            var voucherNos = results.Select(r => r!.VoucherNo!).ToList();
            voucherNos.Should().OnlyHaveUniqueItems();                                   // no two share a number
            voucherNos.Select(v => int.Parse(v.Split('/').Last())).OrderBy(x => x)
                .Should().Equal(Enumerable.Range(1, n));                                 // contiguous 1..n, none lost

            await using var verify = _fixture.CreateFreshDbContext();
            var series = await verify.VoucherTypeNumberSeries
                .FirstAsync(s => s.VoucherTypeId == ids.VoucherTypeId && s.FinancialYearId == ids.FinancialYearId);
            series.LastUsedNumber.Should().Be(n);
        }

        [Fact]
        public async Task PostAsync_Should_Return_Null_When_AlreadyPosted()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            FinanceManagement.Application.JournalMaster.Dto.PostJournalResultDto? second;
            await using (var ctx = _fixture.CreateFreshDbContext())
                second = await CreateRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            second.Should().BeNull();
        }
    }
}
