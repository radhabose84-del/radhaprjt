using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.UserFavoriteMenu.Dto;
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

namespace UserManagement.IntegrationTests.Repositories.UserFavoriteMenu
{
    [Collection("DatabaseCollection")]
    public sealed class UserFavoriteMenuQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserFavoriteMenuQueryRepositoryTests(DbFixture fixture)
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

        private object CreateQueryRepo()
        {
            // UserFavoriteMenuQueryRepository is internal sealed — instantiate via reflection
            var asm = typeof(UserManagement.Infrastructure.Data.ApplicationDbContext).Assembly;
            var type = asm.GetType("UserManagement.Infrastructure.Repositories.UserFavoriteMenu.UserFavoriteMenuQueryRepository")!;
            var conn = new SqlConnection(_fixture.ConnectionString);
            return Activator.CreateInstance(type, conn)!;
        }

        private object CreateCommandRepo(ApplicationDbContext ctx)
        {
            var asm = typeof(UserManagement.Infrastructure.Data.ApplicationDbContext).Assembly;
            var type = asm.GetType("UserManagement.Infrastructure.Repositories.UserFavoriteMenu.UserFavoriteMenuCommandRepository")!;
            return Activator.CreateInstance(type, ctx)!;
        }

        private static async Task<List<UserFavoriteMenuDto>> InvokeGetByUserIdAsync(object repo, int userId)
        {
            var method = repo.GetType().GetMethod("GetByUserIdAsync")!;
            var task = (Task<List<UserFavoriteMenuDto>>)method.Invoke(repo, new object[] { userId })!;
            return await task;
        }

        private static async Task<bool> InvokeBoolAsync(object repo, string methodName, params object[] args)
        {
            var method = repo.GetType().GetMethod(methodName)!;
            var task = (Task<bool>)method.Invoke(repo, args)!;
            return await task;
        }

        private static async Task<int> InvokeCreateFavoriteAsync(object repo, Domain.Entities.UserFavoriteMenu entity)
        {
            var method = repo.GetType().GetMethod("CreateAsync")!;
            var task = (Task<int>)method.Invoke(repo, new object[] { entity })!;
            return await task;
        }

        private async Task<int> SeedModuleAsync(string name = "Test Module")
        {
            await using var ctx = CreateDbContext();
            var module = new Modules { ModuleName = name, IsDeleted = false };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            return module.Id;
        }

        private async Task<int> SeedMenuAsync(int moduleId, string name = "Test Menu")
        {
            await using var ctx = CreateDbContext();
            var menuRepo = new MenuCommandRepository(ctx);
            return await menuRepo.CreateAsync(new Domain.Entities.Menu
            {
                MenuName = name,
                ModuleId = moduleId,
                ParentId = 0,
                MenuUrl = "/test",
                MenuIcon = "icon",
                SortOrder = 1,
                Type = "Standard",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedFavoriteAsync(int userId, int menuId)
        {
            await using var ctx = CreateDbContext();
            var repo = CreateCommandRepo(ctx);
            return await InvokeCreateFavoriteAsync(repo, new Domain.Entities.UserFavoriteMenu
            {
                UserId = userId,
                MenuId = menuId,
                IsActive = Enums.Status.Active,
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

        // --- GetByUserIdAsync ---

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Favorites_For_User()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var menuId = await SeedMenuAsync(moduleId, "Fav Menu");
            await SeedFavoriteAsync(userId: 50, menuId);

            var result = await InvokeGetByUserIdAsync(CreateQueryRepo(), 50);

            result.Should().HaveCount(1);
            result[0].MenuId.Should().Be(menuId);
            result[0].MenuName.Should().Be("Fav Menu");
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Empty_When_User_Has_None()
        {
            await ClearTablesAsync();

            var result = await InvokeGetByUserIdAsync(CreateQueryRepo(), 999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Not_Return_Other_Users_Favorites()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var menuId = await SeedMenuAsync(moduleId);
            await SeedFavoriteAsync(userId: 10, menuId);
            await SeedFavoriteAsync(userId: 20, menuId);

            var result = await InvokeGetByUserIdAsync(CreateQueryRepo(), 10);

            result.Should().HaveCount(1);
        }

        // --- AlreadyFavoritedAsync ---

        [Fact]
        public async Task AlreadyFavoritedAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var menuId = await SeedMenuAsync(moduleId);
            await SeedFavoriteAsync(userId: 33, menuId);

            var exists = await InvokeBoolAsync(CreateQueryRepo(), "AlreadyFavoritedAsync", 33, menuId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyFavoritedAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await InvokeBoolAsync(CreateQueryRepo(), "AlreadyFavoritedAsync", 999, 999);

            exists.Should().BeFalse();
        }

        // --- MenuExistsAndActiveAsync ---

        [Fact]
        public async Task MenuExistsAndActiveAsync_Should_Return_True_For_Active_Menu()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var menuId = await SeedMenuAsync(moduleId);

            var exists = await InvokeBoolAsync(CreateQueryRepo(), "MenuExistsAndActiveAsync", menuId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MenuExistsAndActiveAsync_Should_Return_False_For_NonExistent()
        {
            await ClearTablesAsync();

            var exists = await InvokeBoolAsync(CreateQueryRepo(), "MenuExistsAndActiveAsync", 9999);

            exists.Should().BeFalse();
        }

        // --- FavoriteExistsAsync ---

        [Fact]
        public async Task FavoriteExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var moduleId = await SeedModuleAsync();
            var menuId = await SeedMenuAsync(moduleId);
            await SeedFavoriteAsync(userId: 5, menuId);

            var exists = await InvokeBoolAsync(CreateQueryRepo(), "FavoriteExistsAsync", 5, menuId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task FavoriteExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await InvokeBoolAsync(CreateQueryRepo(), "FavoriteExistsAsync", 1, 1);

            exists.Should().BeFalse();
        }
    }
}
