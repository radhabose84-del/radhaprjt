using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories;
using PurchaseManagement.Infrastructure.Repositories.ExchangeRate;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.ExchangeRate
{
    [Collection("DatabaseCollection")]
    public sealed class ExchangeRateQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ExchangeRateQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ExchangeRateQueryRepository CreateQueryRepo() =>
            new(new Microsoft.Data.SqlClient.SqlConnection(_fixture.ConnectionString));

        private static PurchaseManagement.Domain.Entities.ExchangeRate BuildEntity(
            string baseCcy, string quoteCcy, decimal rate, DateOnly date) =>
            new()
            {
                BaseCurrency = baseCcy,
                QuoteCurrency = quoteCcy,
                Rate = rate,
                EffectiveDate = date,
                Source = "Test",
                IsActive = true
            };

        private async Task SeedEntityAsync(ApplicationDbContext ctx,
            string baseCcy, string quoteCcy, decimal rate, DateOnly date)
        {
            var repo = new ExchangeRateCommandRepository(ctx);
            await repo.UpsertAsync(new[] { BuildEntity(baseCcy, quoteCcy, rate, date) }, CancellationToken.None);
            ctx.ChangeTracker.Clear();
        }

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET LATEST ---

        [Fact]
        public async Task GetLatestAsync_Should_Return_Most_Recent_Rate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var today = DateOnly.FromDateTime(DateTime.Today);
            var yesterday = today.AddDays(-1);

            await SeedEntityAsync(ctx, "USD", "EUR", 0.90m, yesterday);
            await SeedEntityAsync(ctx, "USD", "EUR", 0.92m, today);

            var result = await CreateQueryRepo().GetLatestAsync("USD", "EUR", CancellationToken.None);

            result.Should().NotBeNull();
            result!.Rate.Should().Be(0.92m);
            result.EffectiveDate.Should().Be(today);
        }

        [Fact]
        public async Task GetLatestAsync_Should_Return_Null_When_NoRecord()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateQueryRepo().GetLatestAsync("XYZ", "ABC", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLatestAsync_Should_Return_Correct_CurrencyPair()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var date = DateOnly.FromDateTime(DateTime.Today);

            await SeedEntityAsync(ctx, "USD", "EUR", 0.92m, date);
            await SeedEntityAsync(ctx, "USD", "GBP", 0.79m, date);

            var result = await CreateQueryRepo().GetLatestAsync("USD", "GBP", CancellationToken.None);

            result.Should().NotBeNull();
            result!.QuoteCurrency.Should().Be("GBP");
            result.Rate.Should().Be(0.79m);
        }

        // --- GET BY DATE ---

        [Fact]
        public async Task GetByDateAsync_Should_Return_Record_For_Exact_Date()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var date = new DateOnly(2024, 1, 15);

            await SeedEntityAsync(ctx, "USD", "EUR", 0.91m, date);

            var result = await CreateQueryRepo().GetByDateAsync("USD", "EUR", date, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Rate.Should().Be(0.91m);
        }

        [Fact]
        public async Task GetByDateAsync_Should_Return_Null_For_WrongDate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var date = new DateOnly(2024, 1, 15);

            await SeedEntityAsync(ctx, "USD", "EUR", 0.91m, date);

            var result = await CreateQueryRepo().GetByDateAsync("USD", "EUR", date.AddDays(1), CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
