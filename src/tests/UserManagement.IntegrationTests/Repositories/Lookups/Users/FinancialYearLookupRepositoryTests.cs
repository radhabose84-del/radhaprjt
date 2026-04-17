using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class FinancialYearLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FinancialYearLookupRepositoryTests(DbFixture fixture)
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

        private FinancialYearLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new FinancialYearLookupRepository(conn);
        }

        private async Task<int> SeedAsync(string startYear = "2024", string name = "FY-LKP-2024", DateTime? start = null, DateTime? end = null)
        {
            await using var ctx = CreateDbContext();
            var entity = new UserManagement.Domain.Entities.FinancialYear
            {
                StartYear = startYear,
                StartDate = start ?? new DateTime(2024, 4, 1),
                EndDate = end ?? new DateTime(2025, 3, 31),
                FinYearName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.FinancialYear.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllFinancialYearAsync ---

        [Fact]
        public async Task GetAllFinancialYearAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedAsync("2024", "FY-LKP-2024");

            var results = await CreateLookupRepo().GetAllFinancialYearAsync();

            results.Should().Contain(f => f.FinancialYearName == "FY-LKP-2024");
        }

        [Fact]
        public async Task GetAllFinancialYearAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("2023", "FY-LKP-DEL");

            await using var ctx = CreateDbContext();
            var fy = await ctx.FinancialYear.FirstAsync(f => f.Id == id);
            fy.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllFinancialYearAsync();

            results.Should().NotContain(f => f.FinancialYearName == "FY-LKP-DEL");
        }

        [Fact]
        public async Task GetAllFinancialYearAsync_Should_Order_By_StartDate_Descending()
        {
            await ClearAsync();
            await SeedAsync("2022", "FY-LKP-A", new DateTime(2022, 4, 1), new DateTime(2023, 3, 31));
            await SeedAsync("2024", "FY-LKP-C", new DateTime(2024, 4, 1), new DateTime(2025, 3, 31));
            await SeedAsync("2023", "FY-LKP-B", new DateTime(2023, 4, 1), new DateTime(2024, 3, 31));

            var results = await CreateLookupRepo().GetAllFinancialYearAsync();
            var seeded = results.Where(f => f.FinancialYearName!.StartsWith("FY-LKP-")).ToList();

            seeded.Should().HaveCountGreaterThanOrEqualTo(3);
            seeded[0].FinancialYearName.Should().Be("FY-LKP-C");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Record()
        {
            await ClearAsync();
            var id = await SeedAsync("2024", "FY-LKP-BYID");

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.FinancialYearId.Should().Be(id);
            result.FinancialYearName.Should().Be("FY-LKP-BYID");
            result.StartYear.Should().Be("2024");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateLookupRepo().GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("2023", "FY-LKP-SD");

            await using var ctx = CreateDbContext();
            var fy = await ctx.FinancialYear.FirstAsync(f => f.Id == id);
            fy.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Records()
        {
            await ClearAsync();
            var id1 = await SeedAsync("2022", "FY-LKP-X");
            var id2 = await SeedAsync("2023", "FY-LKP-Y");
            await SeedAsync("2024", "FY-LKP-Z");

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
            await ClearAsync();
            var id = await SeedAsync("2024", "FY-LKP-VALID");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id, 0, -1 });

            results.Should().HaveCount(1);
            results[0].FinancialYearId.Should().Be(id);
        }
    }
}
