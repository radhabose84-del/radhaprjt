using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.City;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class LocationLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LocationLookupRepositoryTests(DbFixture fixture)
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

        private LocationLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ctx = CreateDbContext();

            var countryQuery = new CountryQueryRepository(conn);
            var countryCmd = new CountryCommandRepository(ctx);
            var stateQuery = new UserManagement.Infrastructure.Repositories.StateQueryRepository(conn);
            var stateCmd = new UserManagement.Infrastructure.Repositories.State.StateCommandRepository(ctx);
            var cityQuery = new CityQueryRepository(conn);
            var cityCmd = new CityCommandRepository(ctx);

            return new LocationLookupRepository(
                countryQuery, countryCmd,
                stateQuery, stateCmd,
                cityQuery, cityCmd);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetLocationAsync ---

        [Fact]
        public async Task GetLocationAsync_Should_Create_New_Records_And_Return_Ids()
        {
            await ClearTablesAsync();

            var result = await CreateLookupRepo().GetLocationAsync("Chennai", "Tamil Nadu", "India");

            result.Should().NotBeNull();
            result!.CityId.Should().BeGreaterThan(0);
            result.StateId.Should().BeGreaterThan(0);
            result.CountryId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetLocationAsync_Should_Persist_Created_Records()
        {
            await ClearTablesAsync();

            await CreateLookupRepo().GetLocationAsync("Mumbai", "Maharashtra", "India");

            await using var ctx = CreateDbContext();
            (await ctx.Countries.AnyAsync(c => c.CountryName == "India")).Should().BeTrue();
            (await ctx.States.AnyAsync(s => s.StateName == "Maharashtra")).Should().BeTrue();
            (await ctx.Cities.AnyAsync(c => c.CityName == "Mumbai")).Should().BeTrue();
        }

        [Fact]
        public async Task GetLocationAsync_Should_Reuse_Existing_Records()
        {
            await ClearTablesAsync();
            var first = await CreateLookupRepo().GetLocationAsync("Pune", "Maharashtra", "India");

            var second = await CreateLookupRepo().GetLocationAsync("Pune", "Maharashtra", "India");

            second.Should().NotBeNull();
            second!.CityId.Should().Be(first!.CityId);
            second.StateId.Should().Be(first.StateId);
            second.CountryId.Should().Be(first.CountryId);
        }

        [Fact]
        public async Task GetLocationAsync_Should_Return_Null_For_Empty_City()
        {
            var result = await CreateLookupRepo().GetLocationAsync("", "Tamil Nadu", "India");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLocationAsync_Should_Return_Null_For_Empty_State()
        {
            var result = await CreateLookupRepo().GetLocationAsync("Chennai", "", "India");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLocationAsync_Should_Return_Null_For_Empty_Country()
        {
            var result = await CreateLookupRepo().GetLocationAsync("Chennai", "Tamil Nadu", "");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLocationAsync_Should_Reuse_State_When_Country_Matches()
        {
            await ClearTablesAsync();
            var first = await CreateLookupRepo().GetLocationAsync("Bangalore", "Karnataka", "India");
            var second = await CreateLookupRepo().GetLocationAsync("Mysore", "Karnataka", "India");

            second!.CountryId.Should().Be(first!.CountryId);
            second.StateId.Should().Be(first.StateId);
            second.CityId.Should().NotBe(first.CityId);
        }
    }
}
