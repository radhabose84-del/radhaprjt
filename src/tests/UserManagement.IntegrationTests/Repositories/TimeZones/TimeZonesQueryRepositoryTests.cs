using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.TimeZones;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.TimeZones
{
    [Collection("DatabaseCollection")]
    public sealed class TimeZonesQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TimeZonesQueryRepositoryTests(DbFixture fixture)
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

        private TimeZonesQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new TimeZonesQueryRepository(conn);
        }

        private async Task ClearTableAsync()
        {
            await using var ctx = CreateDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.TimeZones");
        }

        private async Task<int> SeedAsync(string code = "IST", string name = "India Standard Time", string offset = "UTC+05:30")
        {
            await using var ctx = CreateDbContext();
            var tz = new UserManagement.Domain.Entities.TimeZones
            {
                Code = code,
                Name = name,
                Location = "Asia/Kolkata",
                Offset = offset,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.TimeZones.AddAsync(tz);
            await ctx.SaveChangesAsync();
            return tz.Id;
        }

        private async Task SoftDeleteAsync(int id)
        {
            await using var ctx = CreateDbContext();
            var tz = await ctx.TimeZones.FirstAsync(x => x.Id == id);
            tz.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllTimeZonesAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedAsync();

            var (items, total) = await CreateQueryRepo().GetAllTimeZonesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllTimeZonesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();
            await SoftDeleteAsync(id);

            var (items, total) = await CreateQueryRepo().GetAllTimeZonesAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllTimeZonesAsync_Should_Filter_By_SearchTerm_On_Name()
        {
            await ClearTableAsync();
            await SeedAsync("IST", "India Standard Time");
            await SeedAsync("UTC", "Coordinated Universal Time");

            var (items, total) = await CreateQueryRepo().GetAllTimeZonesAsync(1, 10, "India");

            items.Should().HaveCount(1);
            items[0].Name.Should().Be("India Standard Time");
        }

        [Fact]
        public async Task GetAllTimeZonesAsync_Should_Filter_By_SearchTerm_On_Code()
        {
            await ClearTableAsync();
            await SeedAsync("IST", "India Standard Time");
            await SeedAsync("UTC", "Coordinated Universal Time");

            var (items, _) = await CreateQueryRepo().GetAllTimeZonesAsync(1, 10, "UTC");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("UTC");
        }

        [Fact]
        public async Task GetAllTimeZonesAsync_Should_Return_Correct_Pagination()
        {
            await ClearTableAsync();
            await SeedAsync("IST", "India Standard Time");
            await SeedAsync("UTC", "Coordinated Universal Time");
            await SeedAsync("EST", "Eastern Standard Time");

            var (items, total) = await CreateQueryRepo().GetAllTimeZonesAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedAsync("IST", "India Standard Time");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Code.Should().Be("IST");
            result.Name.Should().Be("India Standard Time");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();
            await SoftDeleteAsync(id);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- GET BY NAME ---

        [Fact]
        public async Task GetByTimeZonesNameAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedAsync("IST", "India Standard Time");
            await SeedAsync("EST", "Eastern Standard Time");

            var results = await CreateQueryRepo().GetByTimeZonesNameAsync("India");

            results.Should().HaveCount(1);
            results[0].Name.Should().Be("India Standard Time");
        }

        [Fact]
        public async Task GetByTimeZonesNameAsync_Should_Return_Empty_When_NoMatch()
        {
            await ClearTableAsync();
            await SeedAsync();

            var results = await CreateQueryRepo().GetByTimeZonesNameAsync("Mars");

            results.Should().BeEmpty();
        }
    }
}
