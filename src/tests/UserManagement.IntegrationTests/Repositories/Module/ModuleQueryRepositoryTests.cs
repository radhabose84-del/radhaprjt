using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Module;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Module
{
    [Collection("DatabaseCollection")]
    public sealed class ModuleQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ModuleQueryRepositoryTests(DbFixture fixture)
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

        private ModuleQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ModuleQueryRepository(conn);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string moduleName = "TestModule_QRY")
        {
            var cmdRepo = new ModuleCommandRepository(ctx);
            var module = new Modules { ModuleName = moduleName, IsDeleted = false };
            await cmdRepo.AddModuleAsync(module);
            await cmdRepo.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return module.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllModulesAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "TestModule_QRY_Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllModulesAsync(1, 100, null);

            items.Should().Contain(m => m.ModuleName == "TestModule_QRY_Alpha");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllModulesAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "TestModule_QRY_Searchable");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllModulesAsync(1, 10, "TestModule_QRY_Searchable");

            items.Should().HaveCount(1);
            items[0].ModuleName.Should().Be("TestModule_QRY_Searchable");
        }

        [Fact]
        public async Task GetAllModulesAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "TestModule_QRY_ToDelete");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new ModuleCommandRepository(ctx2);
            await cmdRepo.DeleteModuleAsync(id);

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllModulesAsync(1, 100, "TestModule_QRY_ToDelete");

            items.Should().NotContain(m => m.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetModuleByIdAsync_Should_Return_Correct_Module()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "TestModule_QRY_ById");

            var repo = CreateQueryRepo();
            var result = await repo.GetModuleByIdAsync(id);

            result.Should().NotBeNull();
            result!.ModuleName.Should().Be("TestModule_QRY_ById");
        }

        [Fact]
        public async Task GetModuleByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetModuleByIdAsync(99999);

            result.Should().BeNull();
        }

        // --- GET BY NAME ---

        [Fact]
        public async Task GetModuleByNameAsync_Should_Return_Match()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "TestModule_QRY_ByName");

            var repo = CreateQueryRepo();
            var result = await repo.GetModuleByNameAsync("TestModule_QRY_ByName");

            result.Should().NotBeNull();
            result!.ModuleName.Should().Be("TestModule_QRY_ByName");
        }
    }
}
