using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.GlAccountMaster;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.GlAccountMaster
{
    // US-GL02-07 — SQL-backed favourites + recently-used store (Finance.GlAccountFavourite / GlAccountRecentUse).
    [Collection("DatabaseCollection")]
    public sealed class GlAccountUserPrefRepositoryTests
    {
        private const int UserId = 396;
        private const int CompanyId = 1;

        private readonly DbFixture _fixture;

        public GlAccountUserPrefRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GlAccountUserPrefRepository CreateRepo(ApplicationDbContext ctx)
        {
            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            return new GlAccountUserPrefRepository(ctx, tz.Object);
        }

        // Seed a GL account directly (NOCHECK bypasses its own FK chain) so the favourite/recent FK target exists.
        private async Task<int> SeedGlAccountAsync(string code)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            const string sql = @"
                ALTER TABLE Finance.GlAccountMaster NOCHECK CONSTRAINT ALL;
                INSERT INTO Finance.GlAccountMaster
                    (CompanyId, AccountTypeId, AccountGroupId, AccountCode, AccountName,
                     NormalBalanceId, CurrencyTypeId, SubLedgerTypeId,
                     IsCostCentreMandatory, IsTaxRelevant, IsInterCompany, IsReconciliationRequired,
                     IsActive, IsDeleted, CreatedBy)
                VALUES (1, 1, 1, @Code, @Code, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, new { Code = code });
        }

        private async Task ClearPrefsAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Finance.GlAccountFavourite; DELETE FROM Finance.GlAccountRecentUse;");
        }

        private static string UniqueCode() => $"FZ{Guid.NewGuid():N}".Substring(0, 8);

        [Fact]
        public async Task AddFavourite_Then_GetIds_ReturnsIt()
        {
            await ClearPrefsAsync();
            var glId = await SeedGlAccountAsync(UniqueCode());

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepo(ctx).AddFavouriteAsync(UserId, CompanyId, glId);

            await using var read = _fixture.CreateFreshDbContext();
            var ids = await CreateRepo(read).GetFavouriteAccountIdsAsync(UserId, CompanyId);

            ids.Should().Contain(glId);
        }

        [Fact]
        public async Task RemoveFavourite_SoftDeletes_ThenReAdd_Reactivates()
        {
            await ClearPrefsAsync();
            var glId = await SeedGlAccountAsync(UniqueCode());

            await using (var ctx = _fixture.CreateFreshDbContext()) await CreateRepo(ctx).AddFavouriteAsync(UserId, CompanyId, glId);
            await using (var ctx = _fixture.CreateFreshDbContext()) await CreateRepo(ctx).RemoveFavouriteAsync(UserId, CompanyId, glId);

            await using (var read = _fixture.CreateFreshDbContext())
                (await CreateRepo(read).GetFavouriteAccountIdsAsync(UserId, CompanyId)).Should().NotContain(glId);

            // Re-star reactivates the soft-deleted row (no duplicate — filtered unique index holds).
            await using (var ctx = _fixture.CreateFreshDbContext()) await CreateRepo(ctx).AddFavouriteAsync(UserId, CompanyId, glId);

            await using var read2 = _fixture.CreateFreshDbContext();
            (await CreateRepo(read2).GetFavouriteAccountIdsAsync(UserId, CompanyId)).Should().Contain(glId);
            (await read2.GlAccountFavourite.CountAsync(f => f.UserId == UserId && f.CompanyId == CompanyId && f.GlAccountMasterId == glId))
                .Should().Be(1);
        }

        [Fact]
        public async Task RecordRecent_UpsertsAndIncrementsUseCount()
        {
            await ClearPrefsAsync();
            var glId = await SeedGlAccountAsync(UniqueCode());

            await using (var ctx = _fixture.CreateFreshDbContext()) await CreateRepo(ctx).RecordRecentAsync(UserId, CompanyId, glId);
            await using (var ctx = _fixture.CreateFreshDbContext()) await CreateRepo(ctx).RecordRecentAsync(UserId, CompanyId, glId);

            await using var read = _fixture.CreateFreshDbContext();
            var recent = await CreateRepo(read).GetRecentAsync(UserId, CompanyId, 10);

            recent.Should().ContainSingle(r => r.AccountId == glId);
            recent.Single(r => r.AccountId == glId).UseCount.Should().Be(2);
        }
    }
}
