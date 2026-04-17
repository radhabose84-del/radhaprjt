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
    public sealed class MenuLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MenuLookupRepositoryTests(DbFixture fixture)
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

        private MenuLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MenuLookupRepository(conn);
        }

        private async Task<int> EnsureModuleAsync(string name = "Lookup Module")
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.Modules.FirstOrDefaultAsync(m => m.ModuleName == name);
            if (existing != null) return existing.Id;
            var module = new UserManagement.Domain.Entities.Modules
            {
                ModuleName = name,
                IsDeleted = false
            };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            return module.Id;
        }

        private async Task<int> SeedMenuAsync(int moduleId, string name = "Lookup Menu")
        {
            await using var ctx = CreateDbContext();
            var menu = new UserManagement.Domain.Entities.Menu
            {
                MenuName = name,
                ModuleId = moduleId,
                ParentId = 0,
                MenuUrl = "/lkp",
                MenuIcon = "icon",
                SortOrder = 1,
                Type = "Standard",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Menus.AddAsync(menu);
            await ctx.SaveChangesAsync();
            return menu.Id;
        }

        private async Task ClearMenusAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllMenuAsync ---

        [Fact]
        public async Task GetAllMenuAsync_Should_Return_Seeded_Menu()
        {
            await ClearMenusAsync();
            var moduleId = await EnsureModuleAsync();
            await SeedMenuAsync(moduleId, "Lookup Menu A");

            var results = await CreateLookupRepo().GetAllMenuAsync();

            results.Should().Contain(m => m.MenuName == "Lookup Menu A");
        }

        [Fact]
        public async Task GetAllMenuAsync_Should_Exclude_Inactive()
        {
            await ClearMenusAsync();
            var moduleId = await EnsureModuleAsync();
            var id = await SeedMenuAsync(moduleId, "Lookup Menu Inactive");

            await using var ctx = CreateDbContext();
            var menu = await ctx.Menus.FirstAsync(m => m.Id == id);
            menu.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllMenuAsync();

            results.Should().NotContain(m => m.MenuName == "Lookup Menu Inactive");
        }

        [Fact]
        public async Task GetAllMenuAsync_Should_Exclude_SoftDeleted()
        {
            await ClearMenusAsync();
            var moduleId = await EnsureModuleAsync();
            var id = await SeedMenuAsync(moduleId, "Lookup Menu Deleted");

            await using var ctx = CreateDbContext();
            var menu = await ctx.Menus.FirstAsync(m => m.Id == id);
            menu.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllMenuAsync();

            results.Should().NotContain(m => m.MenuName == "Lookup Menu Deleted");
        }

        [Fact]
        public async Task GetAllMenuAsync_Should_Order_By_MenuName_Ascending()
        {
            await ClearMenusAsync();
            var moduleId = await EnsureModuleAsync();
            await SeedMenuAsync(moduleId, "Lookup Menu Zebra");
            await SeedMenuAsync(moduleId, "Lookup Menu Alpha");
            await SeedMenuAsync(moduleId, "Lookup Menu Beta");

            var results = await CreateLookupRepo().GetAllMenuAsync();
            var seeded = results.Where(m => m.MenuName!.StartsWith("Lookup Menu ")).ToList();

            seeded.Count.Should().BeGreaterThanOrEqualTo(3);
            var indexAlpha = seeded.FindIndex(m => m.MenuName == "Lookup Menu Alpha");
            var indexBeta = seeded.FindIndex(m => m.MenuName == "Lookup Menu Beta");
            var indexZebra = seeded.FindIndex(m => m.MenuName == "Lookup Menu Zebra");
            indexAlpha.Should().BeLessThan(indexBeta);
            indexBeta.Should().BeLessThan(indexZebra);
        }
    }
}
