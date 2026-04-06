using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.DepartmentGroup;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.DepartmentGroup
{
    [Collection("DatabaseCollection")]
    public sealed class DepartmentGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepartmentGroupQueryRepositoryTests(DbFixture fixture)
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

        private DepartmentGroupQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DepartmentGroupQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string code = "DGQRY01", string name = "Query DeptGroup")
        {
            var cmdRepo = new DepartmentGroupCommandRepository(ctx);
            var entity = new Domain.Entities.DepartmentGroup
            {
                DepartmentGroupCode = code,
                DepartmentGroupName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var newId = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return newId;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppData.DepartmentGroup WHERE DepartmentGroupCode LIKE 'DGQRY%'");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllDepartmentGroupAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "DGQRY01", "Query Group Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllDepartmentGroupAsync(1, 100, null);

            items.Should().Contain(d => d.DepartmentGroupCode == "DGQRY01");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllDepartmentGroupAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "DGQRY02", "Searchable Group");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllDepartmentGroupAsync(1, 10, "Searchable");

            items.Should().HaveCount(1);
            items[0].DepartmentGroupName.Should().Be("Searchable Group");
        }

        [Fact]
        public async Task GetAllDepartmentGroupAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "DGQRY03", "Deleted Group");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new DepartmentGroupCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.DepartmentGroup { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllDepartmentGroupAsync(1, 100, "Deleted Group");

            items.Should().NotContain(d => d.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetDepartmentGroupByIdAsync_Should_Return_Correct_Dto()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "DGQRY04", "ById Group");

            var repo = CreateQueryRepo();
            var result = await repo.GetDepartmentGroupByIdAsync(id);

            result.Should().NotBeNull();
            result!.DepartmentGroupCode.Should().Be("DGQRY04");
            result.DepartmentGroupName.Should().Be("ById Group");
        }

        [Fact]
        public async Task GetDepartmentGroupByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetDepartmentGroupByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentGroupByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "DGQRY05", "Soft Deleted Group");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new DepartmentGroupCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.DepartmentGroup { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var result = await repo.GetDepartmentGroupByIdAsync(id);

            result.Should().BeNull();
        }

        // --- BY NAME ---

        [Fact]
        public async Task GetByDepartmentGroupNameAsync_Should_Return_Match()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "DGQRY06", "Named Group Test");

            var repo = CreateQueryRepo();
            var result = await repo.GetByDepartmentGroupNameAsync("Named Group Test");

            result.Should().NotBeNull();
            result!.DepartmentGroupCode.Should().Be("DGQRY06");
        }
    }
}
