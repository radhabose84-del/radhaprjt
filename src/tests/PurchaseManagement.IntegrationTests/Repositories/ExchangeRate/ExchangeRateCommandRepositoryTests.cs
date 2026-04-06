using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.ExchangeRate;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.ExchangeRate
{
    [Collection("DatabaseCollection")]
    public sealed class ExchangeRateCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ExchangeRateCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ExchangeRateCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static PurchaseManagement.Domain.Entities.ExchangeRate BuildEntity(
            string baseCcy = "USD",
            string quoteCcy = "EUR",
            decimal rate = 0.92m,
            DateOnly? date = null) =>
            new()
            {
                BaseCurrency = baseCcy,
                QuoteCurrency = quoteCcy,
                Rate = rate,
                EffectiveDate = date ?? DateOnly.FromDateTime(DateTime.Today),
                Source = "Test",
                IsActive = true
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.ExchangeRate");

        // --- UPSERT (INSERT new) ---

        [Fact]
        public async Task UpsertAsync_Insert_Should_Return_Count_One()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpsertAsync(
                new[] { BuildEntity() }, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpsertAsync_Insert_Should_Persist_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            await CreateRepository(ctx).UpsertAsync(
                new[] { BuildEntity("GBP", "JPY", 186.5m) }, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ExchangeRates.FirstOrDefaultAsync(
                x => x.BaseCurrency == "GBP" && x.QuoteCurrency == "JPY");

            saved.Should().NotBeNull();
            saved!.Rate.Should().Be(186.5m);
        }

        [Fact]
        public async Task UpsertAsync_Insert_Should_Compute_ActualRate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            await CreateRepository(ctx).UpsertAsync(
                new[] { BuildEntity("USD", "INR", 83m) }, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ExchangeRates.FirstOrDefaultAsync(
                x => x.BaseCurrency == "USD" && x.QuoteCurrency == "INR");

            saved!.ActualRate.Should().NotBeNull();
            saved.ActualRate.Should().BeApproximately(1m / 83m, precision: 0.001m);
        }

        // --- UPSERT (UPDATE existing) ---

        [Fact]
        public async Task UpsertAsync_Update_Should_Update_Rate_For_Same_Date()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var date = DateOnly.FromDateTime(DateTime.Today);

            await CreateRepository(ctx).UpsertAsync(
                new[] { BuildEntity("USD", "EUR", 0.92m, date) }, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpsertAsync(
                new[] { BuildEntity("USD", "EUR", 0.95m, date) }, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var rows = await ctx.ExchangeRates
                .Where(x => x.BaseCurrency == "USD" && x.QuoteCurrency == "EUR")
                .ToListAsync();

            rows.Should().HaveCount(1);
            rows[0].Rate.Should().Be(0.95m);
        }

        // --- MULTIPLE ---

        [Fact]
        public async Task UpsertAsync_MultipleSymbols_Should_Return_Correct_Count()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var items = new[]
            {
                BuildEntity("USD", "EUR"),
                BuildEntity("USD", "GBP"),
                BuildEntity("USD", "INR")
            };

            var result = await CreateRepository(ctx).UpsertAsync(items, CancellationToken.None);

            result.Should().Be(3);
        }
    }
}
