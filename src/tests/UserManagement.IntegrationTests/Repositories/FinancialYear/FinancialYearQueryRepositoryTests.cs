using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.FinancialYear;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.FinancialYear
{
    [Collection("DatabaseCollection")]
    public sealed class FinancialYearQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FinancialYearQueryRepositoryTests(DbFixture fixture)
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

        private FinancialYearQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new FinancialYearQueryRepository(conn);
        }

        private async Task<int> SeedAsync(
            ApplicationDbContext ctx,
            string startYear = "2024",
            string finYearName = "FY-QRY-Test")
        {
            var cmdRepo = new FinancialYearCommandRepository(ctx);
            var entity = new Domain.Entities.FinancialYear
            {
                StartYear = startYear,
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2025, 3, 31),
                FinYearName = finYearName,
                StatusId = await EnsureFysStatusAsync(ctx),
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        // Seeds the 'FYS' MiscType + an 'OPEN' MiscMaster status so FinancialYear's required
        // StatusId FK is satisfied, returning the status id.
        private static async Task<int> EnsureFysStatusAsync(ApplicationDbContext ctx)
        {
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "FYS");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "FYS",
                    Description = "Financial Year Status",
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            var status = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "OPEN" && m.MiscTypeId == type.Id);
            if (status == null)
            {
                status = new Domain.Entities.MiscMaster
                {
                    Code = "OPEN",
                    Description = "Open",
                    MiscTypeId = type.Id,
                    SortOrder = 1,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(status);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            return status.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllFinancialYearAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "2024", "FY-QRY-Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllFinancialYearAsync(1, 100, null);

            items.Should().Contain(f => f.FinYearName == "FY-QRY-Alpha");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllFinancialYearAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "2025", "FY-QRY-Searchable");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllFinancialYearAsync(1, 10, "FY-QRY-Searchable");

            items.Should().HaveCount(1);
            items[0].FinYearName.Should().Be("FY-QRY-Searchable");
        }

        [Fact]
        public async Task GetAllFinancialYearAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "2026", "FY-QRY-ToDelete");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new FinancialYearCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.FinancialYear { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllFinancialYearAsync(1, 100, "FY-QRY-ToDelete");

            items.Should().NotContain(f => f.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "2024", "FY-QRY-ById");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.FinYearName.Should().Be("FY-QRY-ById");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "2027", "FY-QRY-SoftDel");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new FinancialYearCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.FinancialYear { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
