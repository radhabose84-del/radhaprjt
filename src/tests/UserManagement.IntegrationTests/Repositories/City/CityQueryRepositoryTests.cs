using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.City;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.State;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.City
{
    [Collection("DatabaseCollection")]
    public sealed class CityQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CityQueryRepositoryTests(DbFixture fixture)
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

        private CityQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CityQueryRepository(conn);
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

        private async Task<(int countryId, int stateId)> SeedCountryAndStateAsync()
        {
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            return (countryId, stateId);
        }

        private async Task SoftDeleteCityAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var entity = await ctx.Cities.FirstAsync(c => c.Id == id);
            entity.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();
        }

        private async Task SetCityInactiveAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var entity = await ctx.Cities.FirstAsync(c => c.Id == id);
            entity.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllCityAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            await SeedCityAsync(stateId);

            var (items, total) = await CreateQueryRepo().GetAllCityAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllCityAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            var cityId = await SeedCityAsync(stateId);
            await SoftDeleteCityAsync(cityId);

            var (items, total) = await CreateQueryRepo().GetAllCityAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllCityAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            await SeedCityAsync(stateId, "CHN", "Chennai");
            await SeedCityAsync(stateId, "BLR", "Bangalore");

            var (items, total) = await CreateQueryRepo().GetAllCityAsync(1, 10, "Chennai");

            items.Should().HaveCount(1);
            items[0].CityName.Should().Be("Chennai");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            var cityId = await SeedCityAsync(stateId, "CHN", "Chennai");

            var result = await CreateQueryRepo().GetByIdAsync(cityId);

            result.Should().NotBeNull();
            result.CityCode.Should().Be("CHN");
            result.CityName.Should().Be("Chennai");
            result.StateId.Should().Be(stateId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            var cityId = await SeedCityAsync(stateId);
            await SoftDeleteCityAsync(cityId);

            Func<Task> act = async () => await CreateQueryRepo().GetByIdAsync(cityId);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_NotFound()
        {
            await ClearTablesAsync();

            Func<Task> act = async () => await CreateQueryRepo().GetByIdAsync(9999);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetByCityNameAsync_Should_Return_Matching_Results()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            await SeedCityAsync(stateId, "CHN", "Chennai");
            await SeedCityAsync(stateId, "CBE", "Coimbatore");

            var results = await CreateQueryRepo().GetByCityNameAsync("Ch");

            results.Should().HaveCount(1);
            results[0].CityName.Should().Be("Chennai");
        }

        [Fact]
        public async Task GetByCityNameAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            var cityId = await SeedCityAsync(stateId, "CHN", "Chennai");
            await SetCityInactiveAsync(cityId);

            var results = await CreateQueryRepo().GetByCityNameAsync("Chennai");

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCityByStateIdAsync_Should_Return_Cities_For_State()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            await SeedCityAsync(stateId, "CHN", "Chennai");
            await SeedCityAsync(stateId, "MDU", "Madurai");

            var results = await CreateQueryRepo().GetCityByStateIdAsync(stateId);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCityByStateIdAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var (_, stateId) = await SeedCountryAndStateAsync();
            var cityId = await SeedCityAsync(stateId, "CHN", "Chennai");
            await SoftDeleteCityAsync(cityId);

            var results = await CreateQueryRepo().GetCityByStateIdAsync(stateId);

            results.Should().BeEmpty();
        }
    }
}
