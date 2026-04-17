using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.MiscTypeMaster;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
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

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string code = "MTM_QRY01", string description = "Query MiscType")
        {
            var cmdRepo = new MiscTypeMasterCommandRepository(ctx);
            var entity = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "MTM_QRY01", "Query MiscType Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllMiscTypeMasterAsync(1, 100, null);

            items.Should().Contain(m => m.MiscTypeCode == "MTM_QRY01");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "MTM_QRY02", "Searchable MiscType");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllMiscTypeMasterAsync(1, 10, "MTM_QRY02");

            items.Should().HaveCount(1);
            items[0].MiscTypeCode.Should().Be("MTM_QRY02");
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "MTM_QRY03", "Deleted MiscType");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new MiscTypeMasterCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.MiscTypeMaster { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllMiscTypeMasterAsync(1, 100, "MTM_QRY03");

            items.Should().NotContain(m => m.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "MTM_QRY04", "ById MiscType");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MiscTypeCode.Should().Be("MTM_QRY04");
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
        public async Task AlreadyExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "MTM_QRY05", "Exists MiscType");

            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync("MTM_QRY05");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync("MTM_NONEXIST_XYZ");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ASYNC ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "MTM_QRY06", "Found MiscType");

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
