using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Currency;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class CurrencyLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CurrencyLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private CurrencyLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CurrencyLookupRepository(conn);
        }

        private async Task<int> SeedCurrencyAsync(string code = "USD", string name = "US Dollar")
        {
            await using var ctx = CreateDbContext();
            var repo = new CurrencyCommandRepository(ctx);
            return await repo.CreateAsync(new UserManagement.Domain.Entities.Currency
            {
                Code = code,
                Name = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Currencies()
        {
            await ClearTableAsync();
            var id1 = await SeedCurrencyAsync("USD", "US Dollar");
            var id2 = await SeedCurrencyAsync("EUR", "Euro");
            await SeedCurrencyAsync("GBP", "British Pound");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
            results.Select(c => c.Code).Should().Contain(new[] { "USD", "EUR" });
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Map_Columns_Correctly()
        {
            await ClearTableAsync();
            var id = await SeedCurrencyAsync("USD", "US Dollar");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id });

            results.Should().HaveCount(1);
            results[0].CurrencyId.Should().Be(id);
            results[0].Code.Should().Be("USD");
            results[0].Name.Should().Be("US Dollar");
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(null!);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Ignore_NonPositive_Ids()
        {
            await ClearTableAsync();
            var id = await SeedCurrencyAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id, 0, -1 });

            results.Should().HaveCount(1);
            results[0].CurrencyId.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id1 = await SeedCurrencyAsync("USD", "US Dollar");
            var id2 = await SeedCurrencyAsync("EUR", "Euro");

            await using var ctx = CreateDbContext();
            var curr = await ctx.Currency.FirstAsync(c => c.Id == id1);
            curr.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(1);
            results[0].CurrencyId.Should().Be(id2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Include_Inactive_When_NotDeleted()
        {
            await ClearTableAsync();
            var id = await SeedCurrencyAsync("USD", "US Dollar");

            await using var ctx = CreateDbContext();
            var curr = await ctx.Currency.FirstAsync(c => c.Id == id);
            curr.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id });

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Deduplicate_Input_Ids()
        {
            await ClearTableAsync();
            var id = await SeedCurrencyAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id, id, id });

            results.Should().HaveCount(1);
        }
    }
}
