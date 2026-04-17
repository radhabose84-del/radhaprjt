using System.Reflection;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Menu;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserFavoriteMenu
{
    [Collection("DatabaseCollection")]
    public sealed class UserFavoriteMenuCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserFavoriteMenuCommandRepositoryTests(DbFixture fixture)
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

        private static object CreateRepository(ApplicationDbContext ctx)
        {
            // UserFavoriteMenuCommandRepository is internal sealed — instantiate via reflection
            var asm = typeof(UserManagement.Infrastructure.Data.ApplicationDbContext).Assembly;
            var type = asm.GetType("UserManagement.Infrastructure.Repositories.UserFavoriteMenu.UserFavoriteMenuCommandRepository")!;
            return Activator.CreateInstance(type, ctx)!;
        }

        private static async Task<int> InvokeCreateAsync(object repo, Domain.Entities.UserFavoriteMenu entity)
        {
            var method = repo.GetType().GetMethod("CreateAsync")!;
            var task = (Task<int>)method.Invoke(repo, new object[] { entity })!;
            return await task;
        }

        private static async Task<bool> InvokeHardDeleteAsync(object repo, int userId, int menuId, CancellationToken ct)
        {
            var method = repo.GetType().GetMethod("HardDeleteAsync")!;
            var task = (Task<bool>)method.Invoke(repo, new object[] { userId, menuId, ct })!;
            return await task;
        }

        private async Task<int> SeedModuleAsync(ApplicationDbContext ctx, string name = "Test Module")
        {
            var module = new Modules { ModuleName = name, IsDeleted = false };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            return module.Id;
        }

        private async Task<int> SeedMenuAsync(ApplicationDbContext ctx, int moduleId, string name = "Test Menu")
        {
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

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private static Domain.Entities.UserFavoriteMenu BuildFavorite(int userId, int menuId) =>
            new Domain.Entities.UserFavoriteMenu
            {
                UserId = userId,
                MenuId = menuId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();
            var menuId = await SeedMenuAsync(ctx, moduleId);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var newId = await InvokeCreateAsync(repo, BuildFavorite(userId: 1, menuId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();
            var menuId = await SeedMenuAsync(ctx, moduleId);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var newId = await InvokeCreateAsync(repo, BuildFavorite(userId: 42, menuId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserFavoriteMenus.FirstOrDefaultAsync(u => u.Id == newId);

            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(42);
            saved.MenuId.Should().Be(menuId);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();
            var menuId = await SeedMenuAsync(ctx, moduleId);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var newId = await InvokeCreateAsync(repo, BuildFavorite(userId: 1, menuId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserFavoriteMenus.FirstOrDefaultAsync(u => u.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task HardDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();
            var menuId = await SeedMenuAsync(ctx, moduleId);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            await InvokeCreateAsync(repo, BuildFavorite(userId: 7, menuId));
            ctx.ChangeTracker.Clear();

            var deleted = await InvokeHardDeleteAsync(repo, 7, menuId, CancellationToken.None);

            deleted.Should().BeTrue();
        }

        [Fact]
        public async Task HardDeleteAsync_Should_Physically_Remove_Record()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();
            var menuId = await SeedMenuAsync(ctx, moduleId);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            await InvokeCreateAsync(repo, BuildFavorite(userId: 11, menuId));
            ctx.ChangeTracker.Clear();

            await InvokeHardDeleteAsync(repo, 11, menuId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var remaining = await ctx.UserFavoriteMenus
                .FirstOrDefaultAsync(u => u.UserId == 11 && u.MenuId == menuId);

            remaining.Should().BeNull();
        }

        [Fact]
        public async Task HardDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);

            var repo = CreateRepository(ctx);
            var deleted = await InvokeHardDeleteAsync(repo, 999, 999, CancellationToken.None);

            deleted.Should().BeFalse();
        }
    }
}
