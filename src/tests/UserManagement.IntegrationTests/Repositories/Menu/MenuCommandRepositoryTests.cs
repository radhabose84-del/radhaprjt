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

namespace UserManagement.IntegrationTests.Repositories.Menu
{
    [Collection("DatabaseCollection")]
    public sealed class MenuCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MenuCommandRepositoryTests(DbFixture fixture)
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

        private MenuCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedModuleAsync(ApplicationDbContext ctx, string name = "Test Module")
        {
            var module = new Modules { ModuleName = name, IsDeleted = false };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            return module.Id;
        }

        private static Domain.Entities.Menu BuildMenu(
            int moduleId,
            string name = "Test Menu",
            int parentId = 0,
            string url = "/test",
            string icon = "icon-test",
            int sortOrder = 1) =>
            new Domain.Entities.Menu
            {
                MenuName = name,
                ModuleId = moduleId,
                ParentId = parentId,
                MenuUrl = url,
                MenuIcon = icon,
                SortOrder = sortOrder,
                Type = "Standard",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Menus");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Modules");
        }

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildMenu(moduleId));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildMenu(moduleId, "Created Menu", 0, "/created", "icon-new", 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Menus.FirstOrDefaultAsync(m => m.Id == id);

            saved.Should().NotBeNull();
            saved!.MenuName.Should().Be("Created Menu");
            saved.ModuleId.Should().Be(moduleId);
            saved.MenuUrl.Should().Be("/created");
            saved.SortOrder.Should().Be(5);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildMenu(moduleId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Menus.FirstOrDefaultAsync(m => m.Id == id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildMenu(moduleId, "Original"));
            ctx.ChangeTracker.Clear();

            var update = new Domain.Entities.Menu
            {
                Id = id,
                MenuName = "Updated Menu",
                ModuleId = moduleId,
                ParentId = 0,
                MenuUrl = "/updated",
                MenuIcon = "icon-updated",
                SortOrder = 10,
                Type = "Updated",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await repo.UpdateAsync(update);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Menus.FirstOrDefaultAsync(m => m.Id == id);
            saved.Should().NotBeNull();
            saved!.MenuName.Should().Be("Updated Menu");
            saved.SortOrder.Should().Be(10);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var update = BuildMenu(moduleId);
            update.Id = 9999;

            var result = await repo.UpdateAsync(update);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildMenu(moduleId));
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Menu { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(id, deleteModel);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Menus.FirstOrDefaultAsync(m => m.Id == id);
            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);

            var repo = CreateRepository(ctx);
            var deleteModel = new Domain.Entities.Menu { IsDeleted = Enums.IsDelete.Deleted };

            var result = await repo.DeleteAsync(9999, deleteModel);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task BulkImportMenuAsync_Should_Persist_All_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var moduleId = await SeedModuleAsync(ctx);
            ctx.ChangeTracker.Clear();

            var repo = CreateRepository(ctx);
            var menus = new List<Domain.Entities.Menu>
            {
                BuildMenu(moduleId, "Bulk Menu 1", 0, "/bulk1"),
                BuildMenu(moduleId, "Bulk Menu 2", 0, "/bulk2"),
                BuildMenu(moduleId, "Bulk Menu 3", 0, "/bulk3")
            };

            var result = await repo.BulkImportMenuAsync(menus);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var count = await ctx.Menus.CountAsync(m => m.ModuleId == moduleId);
            count.Should().Be(3);
        }
    }
}
