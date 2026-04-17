using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.City;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.Infrastructure.Repositories.State;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class CityLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CityLookupRepositoryTests(DbFixture fixture)
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

        private CityLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CityLookupRepository(conn);
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

        private async Task<int> SeedCityAsync(int stateId, string code = "CHN", string name = "Chennai")
        {
            await using var ctx = CreateDbContext();
            var repo = new CityCommandRepository(ctx);
            var created = await repo.CreateAsync(new Cities
            {
                CityCode = code,
                CityName = name,
                StateId = stateId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllCityAsync ---

        [Fact]
        public async Task GetAllCityAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            await SeedCityAsync(stateId);

            var results = await CreateLookupRepo().GetAllCityAsync();

            results.Should().HaveCount(1);
            results[0].CityName.Should().Be("Chennai");
        }

        [Fact]
        public async Task GetAllCityAsync_Should_Exclude_Inactive_City()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            var cityId = await SeedCityAsync(stateId);

            await using var ctx = CreateDbContext();
            var city = await ctx.Cities.FirstAsync(c => c.Id == cityId);
            city.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllCityAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCityAsync_Should_Exclude_SoftDeleted_City()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            var cityId = await SeedCityAsync(stateId);

            await using var ctx = CreateDbContext();
            var city = await ctx.Cities.FirstAsync(c => c.Id == cityId);
            city.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllCityAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCityAsync_Should_Exclude_Cities_With_Inactive_State()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            await SeedCityAsync(stateId);

            await using var ctx = CreateDbContext();
            var state = await ctx.States.FirstAsync(s => s.Id == stateId);
            state.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllCityAsync();

            results.Should().BeEmpty();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_City()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            var cityId = await SeedCityAsync(stateId, "CHN", "Chennai");

            var result = await CreateLookupRepo().GetByIdAsync(cityId);

            result.Should().NotBeNull();
            result!.CityId.Should().Be(cityId);
            result.CityName.Should().Be("Chennai");
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
            var stateId = await SeedStateAsync(countryId);
            var cityId = await SeedCityAsync(stateId);

            await using var ctx = CreateDbContext();
            var city = await ctx.Cities.FirstAsync(c => c.Id == cityId);
            city.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(cityId);

            result.Should().BeNull();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Cities()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            var id1 = await SeedCityAsync(stateId, "CHN", "Chennai");
            var id2 = await SeedCityAsync(stateId, "CBE", "Coimbatore");
            await SeedCityAsync(stateId, "MDU", "Madurai");

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
            var stateId = await SeedStateAsync(countryId);
            var cityId = await SeedCityAsync(stateId);

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { cityId, 0, -1 });

            results.Should().HaveCount(1);
            results[0].CityId.Should().Be(cityId);
        }
    }
}
