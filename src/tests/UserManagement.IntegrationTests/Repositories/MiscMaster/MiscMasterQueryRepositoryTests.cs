using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.MiscMaster;
using UserManagement.Infrastructure.Repositories.MiscTypeMaster;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
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

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "MMQ_TEST_TYPE");
            if (existing != null) return existing.Id;

            var miscType = new UserManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "MMQ_TEST_TYPE",
                Description = "MiscMaster Query Test Type",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return miscType.Id;
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, int miscTypeId, string code = "MMQ_01", string description = "Query MiscMaster")
        {
            var cmdRepo = new MiscMasterCommandRepository(ctx);
            var entity = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx, int miscTypeId) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);
            await SeedAsync(ctx, miscTypeId, "MMQ_01", "Query MiscMaster Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllMiscMasterAsync(1, 100, null);

            items.Should().Contain(m => m.Code == "MMQ_01");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);
            await SeedAsync(ctx, miscTypeId, "MMQ_02", "Searchable MiscMaster");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllMiscMasterAsync(1, 10, "MMQ_02");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("MMQ_02");
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);
            var id = await SeedAsync(ctx, miscTypeId, "MMQ_03", "Deleted MiscMaster");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new MiscMasterCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.MiscMaster { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllMiscMasterAsync(1, 100, "MMQ_03");

            items.Should().NotContain(m => m.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);
            var id = await SeedAsync(ctx, miscTypeId, "MMQ_04", "ById MiscMaster");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("MMQ_04");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(99999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);
            await SeedAsync(ctx, miscTypeId, "MMQ_05", "Exists MiscMaster");

            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync("MMQ_05", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync("MMQ_NONEXIST_XYZ", 99999);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ASYNC ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var miscTypeId = await EnsureMiscTypeAsync(ctx);
            var id = await SeedAsync(ctx, miscTypeId, "MMQ_06", "Found MiscMaster");

            var repo = CreateQueryRepo();
            var found = await repo.NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var found = await repo.NotFoundAsync(99999);

            found.Should().BeFalse();
        }
    }
}
