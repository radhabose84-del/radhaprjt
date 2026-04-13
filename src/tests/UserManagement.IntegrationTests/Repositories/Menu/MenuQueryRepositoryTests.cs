using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Menu;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Menu
{
    [Collection("DatabaseCollection")]
    public sealed class MenuQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MenuQueryRepositoryTests(DbFixture fixture)
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

        private MenuQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MenuQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedModuleAsync(string name = "Test Module")
        {
            await using var ctx = CreateDbContext();
            var module = new Modules { ModuleName = name, IsDeleted = false };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            return module.Id;
        }

        private async Task<int> SeedMenuAsync(
            int moduleId,
            string name = "Test Menu",
            int parentId = 0,
            string url = "/test",
            Enums.Status isActive = Enums.Status.Active)
        {
            await using var ctx = CreateDbContext();
            var repo = new MenuCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Menu
            {
                MenuName = name,
                ModuleId = moduleId,
                ParentId = parentId,
                MenuUrl = url,
                MenuIcon = "icon",
                SortOrder = 1,
                Type = "Standard",
                IsActive = isActive,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var ctx = CreateDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.UserFavoriteMenu");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Menus");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Modules");
        }

        // --- GetAllMenuAsync ---

        [Fact]
        public async Task GetAllMenuAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            await SeedMenuAsync(moduleId);

            var (items, total) = await CreateQueryRepo().GetAllMenuAsync(1, 10, null);

            items.Should().NotBeEmpty();
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMenuAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var id = await SeedMenuAsync(moduleId);

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Menu { IsDeleted = Enums.IsDelete.Deleted };
            await new MenuCommandRepository(ctx).DeleteAsync(id, deleteModel);

            var (items, total) = await CreateQueryRepo().GetAllMenuAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMenuAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            await SeedMenuAsync(moduleId, "Alpha Menu", 0, "/alpha");
            await SeedMenuAsync(moduleId, "Beta Menu", 0, "/beta");

            var (items, total) = await CreateQueryRepo().GetAllMenuAsync(1, 10, "Alpha");

            total.Should().Be(1);
        }

        // --- GetMenuByNameAsync ---

        [Fact]
        public async Task GetMenuByNameAsync_Should_Return_Matching_Menu()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            await SeedMenuAsync(moduleId, "Findable Menu");

            var result = await CreateQueryRepo().GetMenuByNameAsync("Findable Menu");

            result.Should().NotBeNull();
            result.MenuName.Should().Be("Findable Menu");
        }

        [Fact]
        public async Task GetMenuByNameAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMenuByNameAsync("NonExistent");

            result.Should().BeNull();
        }

        // --- GetParentMenuAutoComplete ---

        [Fact]
        public async Task GetParentMenuAutoComplete_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            await SeedMenuAsync(moduleId, "Parent Alpha");
            await SeedMenuAsync(moduleId, "Parent Beta");

            var result = await CreateQueryRepo().GetParentMenuAutoComplete("Alpha");

            result.Should().HaveCount(1);
            result[0].MenuName.Should().Be("Parent Alpha");
        }

        [Fact]
        public async Task GetParentMenuAutoComplete_Should_Handle_Null_SearchPattern()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            await SeedMenuAsync(moduleId);

            var result = await CreateQueryRepo().GetParentMenuAutoComplete(null!);

            result.Should().HaveCount(1);
        }

        // --- FKColumnExistValidation ---

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Menu()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var id = await SeedMenuAsync(moduleId);

            var exists = await CreateQueryRepo().FKColumnExistValidation(id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_NonExistent()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().FKColumnExistValidation(9999);

            exists.Should().BeFalse();
        }

        // --- GetMenusByIds ---

        [Fact]
        public async Task GetMenusByIds_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var id1 = await SeedMenuAsync(moduleId, "Menu 1");
            var id2 = await SeedMenuAsync(moduleId, "Menu 2");

            var result = await CreateQueryRepo().GetMenusByIds(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMenusByIds_Should_Return_Empty_For_Empty_Input()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMenusByIds(new List<int>());

            result.Should().BeEmpty();
        }
    }
}
