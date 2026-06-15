using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.RoleEntitlements;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

namespace UserManagement.IntegrationTests.Repositories.RoleEntitlements
{
    [Collection("DatabaseCollection")]
    public sealed class RoleEntitlementCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RoleEntitlementCommandRepositoryTests(DbFixture fixture)
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

        private RoleEntitlementCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var permMock = new Mock<IPermissionService>(MockBehavior.Loose);
            return new RoleEntitlementCommandRepository(ctx, permMock.Object);
        }

        private async Task<int> EnsureRoleAsync(ApplicationDbContext ctx, string roleName = "TestRole_RE")
        {
            var existing = await ctx.UserRole.FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var cmdRepo = new UserRoleCommandRepository(ctx);
            var role = new UserRole
            {
                RoleName = roleName,
                Description = "Test Role for RoleEntitlement",
                CompanyId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(role);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task<int> EnsureModuleAsync(ApplicationDbContext ctx, string moduleName = "TestMod_RE")
        {
            var existing = await ctx.Modules.FirstOrDefaultAsync(m => m.ModuleName == moduleName);
            if (existing != null) return existing.Id;

            var module = new Modules { ModuleName = moduleName, IsDeleted = false };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return module.Id;
        }

        // --- SAVE ROLE ENTITLEMENTS (create path) ---

        [Fact]
        public async Task SaveRoleEntitlementsAsync_Should_Return_True()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RE_Add");
            var moduleId = await EnsureModuleAsync(ctx, "TestMod_RE_Add");

            var repo = CreateRepository(ctx);

            var roleModules = new List<RoleModule>
            {
                new RoleModule { RoleId = roleId, ModuleId = moduleId }
            };

            var result = await repo.SaveRoleEntitlementsAsync(
                roleId,
                roleModules,
                new List<RoleParent>(),
                new List<RoleChild>(),
                new List<RoleMenuPrivileges>(),
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SaveRoleEntitlementsAsync_Should_Replace_Existing_Not_Duplicate()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RE_Replace");
            var moduleId = await EnsureModuleAsync(ctx, "TestMod_RE_Replace");

            var repo = CreateRepository(ctx);

            var initialModules = new List<RoleModule>
            {
                new RoleModule { RoleId = roleId, ModuleId = moduleId }
            };
            await repo.SaveRoleEntitlementsAsync(
                roleId, initialModules, new List<RoleParent>(),
                new List<RoleChild>(), new List<RoleMenuPrivileges>(), CancellationToken.None);

            ctx.ChangeTracker.Clear();

            // Save again — should replace, not duplicate
            await using var ctx2 = CreateDbContext();
            var permMock2 = new Mock<IPermissionService>(MockBehavior.Loose);
            var repo2 = new RoleEntitlementCommandRepository(ctx2, permMock2.Object);
            var newModules = new List<RoleModule>
            {
                new RoleModule { RoleId = roleId, ModuleId = moduleId }
            };
            var result = await repo2.SaveRoleEntitlementsAsync(
                roleId, newModules, new List<RoleParent>(),
                new List<RoleChild>(), new List<RoleMenuPrivileges>(), CancellationToken.None);

            result.Should().BeTrue();

            ctx2.ChangeTracker.Clear();
            var count = await ctx2.RoleModules.CountAsync(rm => rm.RoleId == roleId);
            count.Should().Be(1);
        }

        // --- MODULE EXISTS ---

        [Fact]
        public async Task ModuleExistsAsync_Should_Return_True_For_Existing_Module()
        {
            await using var ctx = CreateDbContext();
            var moduleId = await EnsureModuleAsync(ctx, "TestMod_RE_Exists");

            var repo = CreateRepository(ctx);
            var exists = await repo.ModuleExistsAsync(moduleId, CancellationToken.None);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ModuleExistsAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var exists = await repo.ModuleExistsAsync(99999, CancellationToken.None);

            exists.Should().BeFalse();
        }
    }
}
