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
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Country
{
    [Collection("DatabaseCollection")]
    public sealed class CountryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CountryQueryRepositoryTests(DbFixture fixture)
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

        private CountryQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CountryQueryRepository(conn);
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

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM AppData.City");
            await conn.ExecuteAsync("DELETE FROM AppData.State");
            await conn.ExecuteAsync("DELETE FROM AppData.Country");
        }

        private async Task SoftDeleteAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var entity = await ctx.Countries.FirstAsync(c => c.Id == id);
            entity.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();
        }

        private async Task SetInactiveAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var entity = await ctx.Countries.FirstAsync(c => c.Id == id);
            entity.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllCountriesAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedCountryAsync();

            var (items, total) = await CreateQueryRepo().GetAllCountriesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllCountriesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedCountryAsync();
            await SoftDeleteAsync(id);

            var (items, total) = await CreateQueryRepo().GetAllCountriesAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllCountriesAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedCountryAsync("IN", "India");
            await SeedCountryAsync("US", "United States");

            var (items, total) = await CreateQueryRepo().GetAllCountriesAsync(1, 10, "India");

            items.Should().HaveCount(1);
            items[0].CountryName.Should().Be("India");
        }

        [Fact]
        public async Task GetAllCountriesAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            await SeedCountryAsync("IN", "India");
            await SeedCountryAsync("US", "United States");
            await SeedCountryAsync("GB", "United Kingdom");

            var (items, total) = await CreateQueryRepo().GetAllCountriesAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedCountryAsync("IN", "India");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.CountryCode.Should().Be("IN");
            result.CountryName.Should().Be("India");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedCountryAsync();
            await SoftDeleteAsync(id);

            Func<Task> act = async () => await CreateQueryRepo().GetByIdAsync(id);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetByCountryNameAsync_Should_Return_Matching_Results()
        {
            await ClearTableAsync();
            await SeedCountryAsync("IN", "India");
            await SeedCountryAsync("ID", "Indonesia");

            var results = await CreateQueryRepo().GetByCountryNameAsync("Ind");

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByCountryNameAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedCountryAsync("IN", "India");
            await SetInactiveAsync(id);

            var results = await CreateQueryRepo().GetByCountryNameAsync("India");

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_NotFound()
        {
            await ClearTableAsync();

            Func<Task> act = async () => await CreateQueryRepo().GetByIdAsync(9999);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
