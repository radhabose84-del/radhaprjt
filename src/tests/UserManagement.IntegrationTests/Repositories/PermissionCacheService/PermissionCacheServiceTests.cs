using Contracts.Common;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Shared.Infrastructure.Services;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.PermissionCacheService
{
    [Collection("DatabaseCollection")]
    public sealed class PermissionCacheServiceTests
    {
        private readonly DbFixture _fixture;

        public PermissionCacheServiceTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private Shared.Infrastructure.Services.PermissionCacheService CreateSut(IMemoryCache? cache = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new Shared.Infrastructure.Services.PermissionCacheService(
                conn,
                cache ?? new MemoryCache(new MemoryCacheOptions()));
        }

        private async Task<int> SeedRoleAsync(string roleName)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM AppSecurity.UserRole WHERE RoleName = @Name AND IsDeleted = 0)
                    INSERT INTO AppSecurity.UserRole (RoleName, Description, CompanyId, BypassDataAccess, IsActive, IsDeleted, CreatedBy, CreatedAt, CreatedByName, CreatedIP)
                    VALUES (@Name, '', 1, 0, 1, 0, 0, GETUTCDATE(), 'test', '127.0.0.1');
                SELECT Id FROM AppSecurity.UserRole WHERE RoleName = @Name AND IsDeleted = 0;";
            return await conn.ExecuteScalarAsync<int>(sql, new { Name = roleName });
        }

        private async Task SeedUserRoleAllocationAsync(int userId, int roleId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(
                "ALTER TABLE AppSecurity.UserRoleAllocation NOCHECK CONSTRAINT FK_UserRoleAllocation_Users_UserId");
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM AppSecurity.UserRoleAllocation WHERE UserId = @UserId AND UserRoleId = @RoleId)
                    INSERT INTO AppSecurity.UserRoleAllocation (UserId, UserRoleId, IsActive)
                    VALUES (@UserId, @RoleId, 1);";
            await conn.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
            await conn.ExecuteAsync(
                "ALTER TABLE AppSecurity.UserRoleAllocation CHECK CONSTRAINT FK_UserRoleAllocation_Users_UserId");
        }

        private async Task SeedMenuPrivilegeAsync(
            int roleId, int menuId,
            bool canView = false, bool canAdd = false, bool canUpdate = false,
            bool canDelete = false, bool canApprove = false, bool canExport = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(
                "ALTER TABLE AppSecurity.RoleMenuPrivilege NOCHECK CONSTRAINT FK_RoleMenuPrivilege_Menus_MenuId");
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM AppSecurity.RoleMenuPrivilege WHERE RoleId = @RoleId AND MenuId = @MenuId AND IsDeleted = 0)
                    INSERT INTO AppSecurity.RoleMenuPrivilege
                        (RoleId, MenuId, CanView, CanAdd, CanUpdate, CanDelete, CanApprove, CanExport, IsActive, IsDeleted, CreatedBy, CreatedAt, CreatedByName, CreatedIP)
                    VALUES (@RoleId, @MenuId, @CanView, @CanAdd, @CanUpdate, @CanDelete, @CanApprove, @CanExport, 1, 0, 0, GETUTCDATE(), 'test', '127.0.0.1');";
            await conn.ExecuteAsync(sql, new
            {
                RoleId = roleId, MenuId = menuId,
                CanView = canView, CanAdd = canAdd, CanUpdate = canUpdate,
                CanDelete = canDelete, CanApprove = canApprove, CanExport = canExport
            });
            await conn.ExecuteAsync(
                "ALTER TABLE AppSecurity.RoleMenuPrivilege CHECK CONSTRAINT FK_RoleMenuPrivilege_Menus_MenuId");
        }

        private async Task ClearPermissionDataAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM AppSecurity.RoleMenuPrivilege WHERE CreatedBy = 0 AND RoleName IS NULL OR 1=1", commandTimeout: 10);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Guard tests (no DB needed)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task HasPermissionAsync_Returns_False_When_UserId_Is_Zero()
        {
            var sut = CreateSut();
            var result = await sut.HasPermissionAsync(0, 10, PermissionType.CanAdd);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasPermissionAsync_Returns_False_When_MenuId_Is_Zero()
        {
            var sut = CreateSut();
            var result = await sut.HasPermissionAsync(1, 0, PermissionType.CanAdd);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasPermissionAsync_Returns_False_When_No_Role_Assignment()
        {
            var sut = CreateSut();
            // userId 99999 has no role assignment in test DB
            var result = await sut.HasPermissionAsync(99999, 99999, PermissionType.CanView);
            result.Should().BeFalse();
        }

        // ─────────────────────────────────────────────────────────────────────
        // DB-backed permission tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task HasPermissionAsync_Returns_True_When_CanAdd_Granted()
        {
            var roleId = await SeedRoleAsync("perm_test_role_add");
            await SeedUserRoleAllocationAsync(5001, roleId);
            await SeedMenuPrivilegeAsync(roleId, 5001, canAdd: true);

            var sut = CreateSut();
            var result = await sut.HasPermissionAsync(5001, 5001, PermissionType.CanAdd);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasPermissionAsync_Returns_False_When_CanAdd_Is_False()
        {
            var roleId = await SeedRoleAsync("perm_test_role_noadd");
            await SeedUserRoleAllocationAsync(5002, roleId);
            await SeedMenuPrivilegeAsync(roleId, 5002, canAdd: false, canView: true);

            var sut = CreateSut();
            var result = await sut.HasPermissionAsync(5002, 5002, PermissionType.CanAdd);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(PermissionType.CanView,    true,  false, false, false, false, false)]
        [InlineData(PermissionType.CanAdd,     false, true,  false, false, false, false)]
        [InlineData(PermissionType.CanUpdate,  false, false, true,  false, false, false)]
        [InlineData(PermissionType.CanDelete,  false, false, false, true,  false, false)]
        [InlineData(PermissionType.CanApprove, false, false, false, false, true,  false)]
        [InlineData(PermissionType.CanExport,  false, false, false, false, false, true)]
        public async Task HasPermissionAsync_Each_PermissionType_Maps_Correctly(
            PermissionType permType,
            bool canView, bool canAdd, bool canUpdate,
            bool canDelete, bool canApprove, bool canExport)
        {
            var roleId = await SeedRoleAsync($"perm_test_type_{permType}");
            var userId = 5010 + (int)permType;
            var menuId = 5010 + (int)permType;
            await SeedUserRoleAllocationAsync(userId, roleId);
            await SeedMenuPrivilegeAsync(roleId, menuId,
                canView, canAdd, canUpdate, canDelete, canApprove, canExport);

            var sut = CreateSut();
            var result = await sut.HasPermissionAsync(userId, menuId, permType);
            result.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Caching behaviour
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task HasPermissionAsync_Returns_Same_Result_On_Second_Call()
        {
            var roleId = await SeedRoleAsync("perm_test_cache_hit");
            await SeedUserRoleAllocationAsync(5020, roleId);
            await SeedMenuPrivilegeAsync(roleId, 5020, canView: true);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var sut = CreateSut(cache);

            var first  = await sut.HasPermissionAsync(5020, 5020, PermissionType.CanView);
            var second = await sut.HasPermissionAsync(5020, 5020, PermissionType.CanView);

            first.Should().BeTrue();
            second.Should().BeTrue();
        }

        [Fact]
        public void InvalidateCache_Does_Not_Throw_When_No_Cache_Entry_Exists()
        {
            var sut = CreateSut();
            var act = () => sut.InvalidateCache(99999);
            act.Should().NotThrow();
        }

        [Fact]
        public async Task InvalidateCache_Allows_Fresh_Lookup_After_Invalidation()
        {
            var roleId = await SeedRoleAsync("perm_test_invalidate");
            await SeedUserRoleAllocationAsync(5030, roleId);
            await SeedMenuPrivilegeAsync(roleId, 5030, canDelete: true);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var sut = CreateSut(cache);

            // Populate cache
            await sut.HasPermissionAsync(5030, 5030, PermissionType.CanDelete);

            // Invalidate
            sut.InvalidateCache(5030);

            // Re-query should still return true (data hasn't changed, just cache was cleared)
            var result = await sut.HasPermissionAsync(5030, 5030, PermissionType.CanDelete);
            result.Should().BeTrue();
        }
    }
}
