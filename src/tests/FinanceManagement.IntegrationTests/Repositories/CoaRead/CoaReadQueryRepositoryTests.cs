using System.Diagnostics;
using FinanceManagement.Infrastructure.Repositories.CoaRead;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using Microsoft.Data.SqlClient;

namespace FinanceManagement.IntegrationTests.Repositories.CoaRead
{
    // US-GL02-16 — COA Read API repository against the real test DB. Reuses JournalTestSeed for a
    // coherent GL account graph (company 1; accounts 5200101 / 2200101).
    [Collection("DatabaseCollection")]
    public sealed class CoaReadQueryRepositoryTests
    {
        private const int CompanyId = 1;
        private readonly DbFixture _fixture;
        public CoaReadQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CoaReadQueryRepository Repo() => new(new SqlConnection(_fixture.ConnectionString));

        private async Task SeedAsync()
        {
            await _fixture.ClearAllTablesAsync();
            await JournalTestSeed.SeedGraphAsync(_fixture);
        }

        // ── AC1 — get-by-code + <100ms ──────────────────────────────────────────
        [Fact]
        public async Task GetByCode_ReturnsAccount_WithStatus()
        {
            await SeedAsync();
            var dto = await Repo().GetByCodeAsync(CompanyId, "5200101", CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.AccountName.Should().Be("Salaries & Wages");
            dto.IsActive.Should().BeTrue();
            dto.CurrencyTypeCode.Should().Be("INR");
        }

        [Fact]
        public async Task GetByCode_Unknown_ReturnsNull()
        {
            await SeedAsync();
            (await Repo().GetByCodeAsync(CompanyId, "NOPE999", CancellationToken.None)).Should().BeNull();
        }

        [Fact]
        public async Task GetByCode_RespondsUnder100ms()
        {
            await SeedAsync();
            var repo = Repo();
            await repo.GetByCodeAsync(CompanyId, "5200101", CancellationToken.None); // warm up connection/plan

            var sw = Stopwatch.StartNew();
            var dto = await repo.GetByCodeAsync(CompanyId, "5200101", CancellationToken.None);
            sw.Stop();

            dto.Should().NotBeNull();
            sw.Elapsed.TotalMilliseconds.Should().BeLessThan(100, "AC1 — single-account lookup must be < 100ms");
        }

        // ── AC5 — search by type/group with status ──────────────────────────────
        [Fact]
        public async Task Search_ByType_ReturnsMatchesWithStatus()
        {
            await SeedAsync();
            var repo = Repo();
            var seeded = await repo.GetByCodeAsync(CompanyId, "5200101", CancellationToken.None);

            var rows = await repo.SearchByTypeGroupAsync(CompanyId, seeded!.AccountTypeId, null, false, CancellationToken.None);

            rows.Should().Contain(r => r.AccountCode == "5200101");
            rows.Should().OnlyContain(r => r.AccountTypeId == seeded.AccountTypeId);
            rows.Should().OnlyContain(r => r.IsActive); // seed accounts are active
        }

        // ── AC2 — posting info building block ───────────────────────────────────
        [Fact]
        public async Task GetPostingInfoByCode_CarriesCurrencyAndCcFlag()
        {
            await SeedAsync();
            var info = await Repo().GetPostingInfoByCodeAsync(CompanyId, "5200101", CancellationToken.None);

            info.Should().NotBeNull();
            info!.IsActive.Should().BeTrue();
            info.CurrencyTypeCode.Should().Be("INR");
            info.IsCostCentreMandatory.Should().BeFalse();
        }
    }
}
