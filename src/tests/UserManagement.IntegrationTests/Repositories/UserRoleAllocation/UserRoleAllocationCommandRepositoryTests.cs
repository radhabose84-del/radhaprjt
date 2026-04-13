using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationCommandRepository;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserRoleAllocation
{
    [Collection("DatabaseCollection")]
    public sealed class UserRoleAllocationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserRoleAllocationCommandRepositoryTests(DbFixture fixture)
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

        private UserRoleAllocationCommandRepository CreateRepository(
            ApplicationDbContext ctx,
            Mock<IUserRoleAllocationQueryRepository>? queryMock = null)
        {
            queryMock ??= new Mock<IUserRoleAllocationQueryRepository>(MockBehavior.Loose);
            return new UserRoleAllocationCommandRepository(ctx, queryMock.Object);
        }

        private async Task<int> SeedUserRoleAsync(ApplicationDbContext ctx, string roleName)
        {
            var repo = new UserRoleCommandRepository(ctx);
            var created = await repo.CreateAsync(new UserRole
            {
                RoleName = roleName,
                Description = "Test Role for URA",
                CompanyId = 1,
                BypassDataAccess = false,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();
            return created.Id;
        }

        /// <summary>
        /// Seeds a minimal User row via raw SQL (bypassing EF DepartmentId FK).
        /// Returns the seeded UserId.
        /// </summary>
        private async Task<int> SeedUserAsync(int userId, string userName)
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();

            const string sql = @"
-- Disable FK constraints on Users for test
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AppSecurity.Users'))
BEGIN
    DECLARE @sql nvarchar(max) = N'';
    SELECT @sql = @sql + N'ALTER TABLE AppSecurity.Users NOCHECK CONSTRAINT [' + fk.name + N'];' + CHAR(10)
    FROM sys.foreign_keys fk
    WHERE fk.parent_object_id = OBJECT_ID('AppSecurity.Users');
    EXEC sp_executesql @sql;
END

-- Ensure a clean user row
DELETE FROM AppSecurity.UserRoleAllocation WHERE UserId = @UserId;
DELETE FROM AppSecurity.Users WHERE UserId = @UserId;

SET IDENTITY_INSERT AppSecurity.Users ON;

INSERT INTO AppSecurity.Users
(
    UserId, FirstName, LastName, UserName, DepartmentId, IsActive,
    PasswordHash, UserType, Mobile, EmailId, IsFirstTimeUser, IsDeleted,
    UserGroupId, EntityId, PartyId, CreatedAt, CreatedBy, CreatedByName, CreatedIp, IsLocked
)
VALUES
(
    @UserId, 'Test', 'User', @UserName, 1, 1,
    'x', 0, '1', @UserName + '@test.com', 0, 0,
    NULL, 1, NULL, SYSUTCDATETIME(), 1, 'seed', '127.0.0.1', 0
);

SET IDENTITY_INSERT AppSecurity.Users OFF;
";
            await cnn.ExecuteAsync(sql, new { UserId = userId, UserName = userName });
            return userId;
        }

        private async Task ClearUserRoleAllocationsAsync(int userId)
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync(
                "DELETE FROM AppSecurity.UserRoleAllocation WHERE UserId = @UserId",
                new { UserId = userId });
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Persist_New_Allocation()
        {
            await using var ctx = CreateDbContext();
            var roleId = await SeedUserRoleAsync(ctx, "TestRole_URAC_Create");
            var userId = await SeedUserAsync(9001, "ura_create_user");
            await ClearUserRoleAllocationsAsync(userId);

            var repo = CreateRepository(ctx);
            var allocation = new UserManagement.Domain.Entities.UserRoleAllocation
            {
                UserRoleId = roleId,
                UserId = userId,
                IsActive = 1
            };

            await repo.CreateAsync(allocation);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.UserRoleAllocations
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UserRoleId == roleId);

            saved.Should().NotBeNull();
            saved!.IsActive.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_Should_Assign_Generated_Id()
        {
            await using var ctx = CreateDbContext();
            var roleId = await SeedUserRoleAsync(ctx, "TestRole_URAC_IdGen");
            var userId = await SeedUserAsync(9002, "ura_idgen_user");
            await ClearUserRoleAllocationsAsync(userId);

            var repo = CreateRepository(ctx);
            var allocation = new UserManagement.Domain.Entities.UserRoleAllocation
            {
                UserRoleId = roleId,
                UserId = userId,
                IsActive = 1
            };

            await repo.CreateAsync(allocation);

            allocation.Id.Should().BeGreaterThan(0);
        }

        // --- ADD RANGE ---

        [Fact]
        public async Task AddRangeAsync_Should_Persist_Multiple_Allocations()
        {
            await using var ctx = CreateDbContext();
            var roleId1 = await SeedUserRoleAsync(ctx, "TestRole_URAC_Range1");
            var roleId2 = await SeedUserRoleAsync(ctx, "TestRole_URAC_Range2");
            var userId = await SeedUserAsync(9003, "ura_range_user");
            await ClearUserRoleAllocationsAsync(userId);

            var repo = CreateRepository(ctx);
            var allocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>
            {
                new() { UserRoleId = roleId1, UserId = userId, IsActive = 1 },
                new() { UserRoleId = roleId2, UserId = userId, IsActive = 1 }
            };

            await repo.AddRangeAsync(allocations);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.UserRoleAllocations
                .Where(x => x.UserId == userId)
                .ToListAsync();

            saved.Should().HaveCount(2);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            var roleId = await SeedUserRoleAsync(ctx, "TestRole_URAC_Update");
            var userId = await SeedUserAsync(9004, "ura_update_user");
            await ClearUserRoleAllocationsAsync(userId);

            var repo = CreateRepository(ctx);
            var allocation = new UserManagement.Domain.Entities.UserRoleAllocation
            {
                UserRoleId = roleId,
                UserId = userId,
                IsActive = 1
            };
            await repo.CreateAsync(allocation);
            ctx.ChangeTracker.Clear();

            // Reload and modify
            var toUpdate = await ctx.UserRoleAllocations
                .FirstAsync(x => x.UserId == userId && x.UserRoleId == roleId);
            toUpdate.IsActive = 0;

            await repo.UpdateAsync(toUpdate);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.UserRoleAllocations
                .FirstOrDefaultAsync(x => x.Id == toUpdate.Id);

            updated.Should().NotBeNull();
            updated!.IsActive.Should().Be(0);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Remove_Allocation_When_Found()
        {
            await using var ctx = CreateDbContext();
            var roleId = await SeedUserRoleAsync(ctx, "TestRole_URAC_Delete");
            var userId = await SeedUserAsync(9005, "ura_delete_user");
            await ClearUserRoleAllocationsAsync(userId);

            // Create allocation directly via EF to get its Id (bypass broken query repo)
            var allocation = new UserManagement.Domain.Entities.UserRoleAllocation
            {
                UserRoleId = roleId,
                UserId = userId,
                IsActive = 1
            };
            await ctx.UserRoleAllocations.AddAsync(allocation);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            // Mock the query repo to return the allocation (the real one has broken SQL)
            var queryMock = new Mock<IUserRoleAllocationQueryRepository>(MockBehavior.Loose);
            queryMock.Setup(q => q.GetByIdAsync(allocation.Id))
                .ReturnsAsync(new UserManagement.Domain.Entities.UserRoleAllocation
                {
                    Id = allocation.Id,
                    UserRoleId = roleId,
                    UserId = userId,
                    IsActive = 1
                });

            var repo = CreateRepository(ctx, queryMock);
            await repo.DeleteAsync(allocation.Id);

            ctx.ChangeTracker.Clear();
            await using var verifyCtx = CreateDbContext();
            var deleted = await verifyCtx.UserRoleAllocations
                .FirstOrDefaultAsync(x => x.Id == allocation.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Be_NoOp_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var queryMock = new Mock<IUserRoleAllocationQueryRepository>(MockBehavior.Loose);
            queryMock.Setup(q => q.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((UserManagement.Domain.Entities.UserRoleAllocation?)null);

            var repo = CreateRepository(ctx, queryMock);

            Func<Task> act = async () => await repo.DeleteAsync(999999);

            await act.Should().NotThrowAsync();
        }
    }
}
