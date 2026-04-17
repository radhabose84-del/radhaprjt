using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.Infrastructure.Repositories.State;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class StateLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StateLookupRepositoryTests(DbFixture fixture)
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

        private StateLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new StateLookupRepository(conn);
        }

        private async Task<int> SeedCountryAsync(string code = "IN", string name = "India")
        {
            await using var ctx = CreateDbContext();
            var repo = new CountryCommandRepository(ctx);
            var created = await repo.CreateAsync(new Countries
            {
                CountryCode = code,
                CountryName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task<int> SeedStateAsync(int countryId, string code = "TN", string name = "Tamil Nadu")
        {
            await using var ctx = CreateDbContext();
            var repo = new StateCommandRepository(ctx);
            var created = await repo.CreateAsync(new States
            {
                StateCode = code,
                StateName = name,
                CountryId = countryId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllStatesAsync ---

        [Fact]
        public async Task GetAllStatesAsync_Should_Return_Seeded_State()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            await SeedStateAsync(countryId);

            var results = await CreateLookupRepo().GetAllStatesAsync();

            results.Should().HaveCount(1);
            results[0].StateName.Should().Be("Tamil Nadu");
        }

        [Fact]
        public async Task GetAllStatesAsync_Should_Exclude_Inactive_State()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var id = await SeedStateAsync(countryId);

            await using var ctx = CreateDbContext();
            var state = await ctx.States.FirstAsync(s => s.Id == id);
            state.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllStatesAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllStatesAsync_Should_Exclude_SoftDeleted_State()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var id = await SeedStateAsync(countryId);

            await using var ctx = CreateDbContext();
            var state = await ctx.States.FirstAsync(s => s.Id == id);
            state.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllStatesAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllStatesAsync_Should_Exclude_States_With_Inactive_Country()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            await SeedStateAsync(countryId);

            await using var ctx = CreateDbContext();
            var country = await ctx.Countries.FirstAsync(c => c.Id == countryId);
            country.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllStatesAsync();

            results.Should().BeEmpty();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_State()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var id = await SeedStateAsync(countryId, "KA", "Karnataka");

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.StateId.Should().Be(id);
            result.StateName.Should().Be("Karnataka");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var id = await SeedStateAsync(countryId);

            await using var ctx = CreateDbContext();
            var state = await ctx.States.FirstAsync(s => s.Id == id);
            state.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_States()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var id1 = await SeedStateAsync(countryId, "TN", "Tamil Nadu");
            var id2 = await SeedStateAsync(countryId, "KA", "Karnataka");
            await SeedStateAsync(countryId, "KL", "Kerala");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Ignore_NonPositive_Ids()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var id = await SeedStateAsync(countryId);

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id, 0, -1 });

            results.Should().HaveCount(1);
            results[0].StateId.Should().Be(id);
        }
    }
}
