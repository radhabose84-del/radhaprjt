using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;

namespace FinanceManagement.IntegrationTests.Repositories.Journal
{
    [Collection("DatabaseCollection")]
    public sealed class JournalQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public JournalQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private JournalQueryRepository CreateQueryRepo()
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Test Company" } });

            var fy = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            fy.Setup(f => f.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto>
                {
                    new() { FinancialYearId = 3, FinancialYearName = "2026-27", IsActive = true,
                            StartDate = new DateTime(2000, 1, 1), EndDate = new DateTime(2100, 1, 1) }
                });

            return new JournalQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object, fy.Object);
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedDraftAsync(SeededIds ids)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));
        }

        [Fact]
        public async Task GetStatusIdAsync_Should_Resolve_Draft()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var statusId = await CreateQueryRepo().GetStatusIdAsync("DRAFT");

            statusId.Should().Be(ids.StatusDraftId);
        }

        [Fact]
        public async Task GetSourceIdAsync_Should_Resolve_Manual()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var sourceId = await CreateQueryRepo().GetSourceIdAsync("MANUAL");

            sourceId.Should().Be(ids.SourceManualId);
        }

        [Fact]
        public async Task GetOpenPeriodByDateAsync_Should_Resolve_Period_In_Range()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var period = await CreateQueryRepo().GetOpenPeriodByDateAsync(1, new DateOnly(2026, 6, 15));

            period.Should().NotBeNull();
            period!.Value.PeriodId.Should().Be(ids.AccountingPeriodId);
            period.Value.FinancialYearId.Should().Be(3);
        }

        [Fact]
        public async Task GetOpenPeriodByDateAsync_Should_Return_Null_OutOfRange()
        {
            await ClearTableAsync();
            await JournalTestSeed.SeedGraphAsync(_fixture);

            var period = await CreateQueryRepo().GetOpenPeriodByDateAsync(1, new DateOnly(2026, 9, 15));

            period.Should().BeNull();
        }

        [Fact]
        public async Task FkValidators_Should_Return_True_For_Seeded()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var repo = CreateQueryRepo();

            (await repo.VoucherTypeExistsAsync(ids.VoucherTypeId, 1)).Should().BeTrue();
            (await repo.GlAccountExistsAsync(ids.GlAccountDrId, 1)).Should().BeTrue();
            (await repo.CostCentreExistsAsync(ids.CostCentreId)).Should().BeTrue();
            (await repo.ProfitCentreExistsAsync(ids.ProfitCentreId)).Should().BeTrue();
            (await repo.CurrencyExistsAsync(ids.CurrencyId)).Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Lines()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var id = await SeedDraftAsync(ids);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Lines.Should().HaveCount(2);
            dto.CompanyName.Should().Be("Test Company");
            dto.FinancialYearName.Should().Be("2026-27");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Draft()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            await SeedDraftAsync(ids);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task State_Flags_Should_Reflect_Draft_Then_Posted()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var id = await SeedDraftAsync(ids);
            var repo = CreateQueryRepo();

            (await repo.IsDraftAsync(id)).Should().BeTrue();
            (await repo.IsBalancedAsync(id)).Should().BeTrue();
            (await repo.IsPeriodOpenAsync(id)).Should().BeTrue();
            (await repo.IsPostedAsync(id)).Should().BeFalse();

            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            (await repo.IsPostedAsync(id)).Should().BeTrue();
            (await repo.IsDraftAsync(id)).Should().BeFalse();
        }
    }
}
