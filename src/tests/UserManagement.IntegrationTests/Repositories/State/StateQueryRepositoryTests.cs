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
using UserManagement.Infrastructure.Repositories;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.State;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.State
{
    [Collection("DatabaseCollection")]
    public sealed class StateQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StateQueryRepositoryTests(DbFixture fixture)
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

        private StateQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new StateQueryRepository(conn);
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

        private async Task SoftDeleteStateAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var entity = await ctx.States.FirstAsync(s => s.Id == id);
            entity.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();
        }

        private async Task SetStateInactiveAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var entity = await ctx.States.FirstAsync(s => s.Id == id);
            entity.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllStatesAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            await SeedStateAsync(countryId);

            var (items, total) = await CreateQueryRepo().GetAllStatesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllStatesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            await SoftDeleteStateAsync(stateId);

            var (items, total) = await CreateQueryRepo().GetAllStatesAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllStatesAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            await SeedStateAsync(countryId, "TN", "Tamil Nadu");
            await SeedStateAsync(countryId, "KA", "Karnataka");

            var (items, total) = await CreateQueryRepo().GetAllStatesAsync(1, 10, "Tamil");

            items.Should().HaveCount(1);
            items[0].StateName.Should().Be("Tamil Nadu");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId, "TN", "Tamil Nadu");

            var result = await CreateQueryRepo().GetByIdAsync(stateId);

            result.Should().NotBeNull();
            result.StateCode.Should().Be("TN");
            result.StateName.Should().Be("Tamil Nadu");
            result.CountryId.Should().Be(countryId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId);
            await SoftDeleteStateAsync(stateId);

            Func<Task> act = async () => await CreateQueryRepo().GetByIdAsync(stateId);

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
        public async Task GetByStateNameAsync_Should_Return_Matching_Results()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            await SeedStateAsync(countryId, "TN", "Tamil Nadu");
            await SeedStateAsync(countryId, "TE", "Telangana");

            var results = await CreateQueryRepo().GetByStateNameAsync("Ta");

            results.Should().HaveCount(1);
            results[0].StateName.Should().Be("Tamil Nadu");
        }

        [Fact]
        public async Task GetByStateNameAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            var stateId = await SeedStateAsync(countryId, "TN", "Tamil Nadu");
            await SetStateInactiveAsync(stateId);

            var results = await CreateQueryRepo().GetByStateNameAsync("Tamil");

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStateByCountryIdAsync_Should_Return_States_For_Country()
        {
            await ClearTablesAsync();
            var countryId = await SeedCountryAsync();
            await SeedStateAsync(countryId, "TN", "Tamil Nadu");
            await SeedStateAsync(countryId, "KA", "Karnataka");

            var results = await CreateQueryRepo().GetStateByCountryIdAsync(countryId);

            results.Should().HaveCount(2);
        }
    }
}
