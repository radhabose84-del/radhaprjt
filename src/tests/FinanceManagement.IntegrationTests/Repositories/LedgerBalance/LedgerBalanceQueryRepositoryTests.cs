using Microsoft.Data.SqlClient;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.LedgerBalances;

namespace FinanceManagement.IntegrationTests.Repositories.LedgerBalance
{
    [Collection("DatabaseCollection")]
    public sealed class LedgerBalanceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LedgerBalanceQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private LedgerBalanceQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        [Fact]
        public async Task GetAllAsync_Returns_Balances_With_Account_Type_And_Group_Info()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Create + post a journal so LedgerBalance rows exist.
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids, amount: 1000m));
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            var (items, total) = await CreateRepo().GetAllAsync(1, 50, ids.CompanyId, null, null, null, null, null, null, null);

            total.Should().Be(2);   // debit + credit buckets

            var dr = items.First(x => x.GlAccountId == ids.GlAccountDrId);
            dr.AccountCode.Should().Be("5200101");
            dr.AccountName.Should().Be("Salaries & Wages");
            dr.AccountTypeName.Should().Be("Expense");      // from JournalTestSeed AccountTypeMaster
            dr.GroupCode.Should().Be("JVGRP");              // from JournalTestSeed AccountGroup
            dr.GroupName.Should().Be("JV Test Group");
            dr.GroupLevel.Should().Be(1);                   // hierarchy level
            dr.IsLeaf.Should().BeTrue();
            dr.ParentAccountGroupId.Should().BeNull();      // top-level group → no parent
            dr.DrTotal.Should().Be(1000m);
            dr.Balance.Should().Be(1000m);
        }

        [Fact]
        public async Task GetAllAsync_Filters_By_GlAccount()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids, amount: 1000m));
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            var (items, total) = await CreateRepo().GetAllAsync(1, 50, ids.CompanyId, null, null, ids.GlAccountCrId, null, null, null, null);

            total.Should().Be(1);
            items.Should().ContainSingle().Which.GlAccountId.Should().Be(ids.GlAccountCrId);
            items[0].CrTotal.Should().Be(1000m);
            items[0].Balance.Should().Be(-1000m);
        }
    }
}
