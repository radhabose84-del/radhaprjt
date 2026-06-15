using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserRoleAllocationEntity = UserManagement.Domain.Entities.UserRoleAllocation;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.AccessPolicy;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.IntegrationTests.Repositories.AccessPolicy
{
    [Collection("DatabaseCollection")]
    public sealed class AccessPolicyEvaluationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AccessPolicyEvaluationRepositoryTests(DbFixture fixture)
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

        private AccessPolicyEvaluationRepository CreateEvalRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        /// <summary>
        /// Inserts a minimal User row directly via SQL with FK constraints temporarily disabled.
        /// Needed because User has deep dependencies (Department, Entity, etc.) in EF Core.
        /// </summary>
        private async Task<int> SeedUserAsync(string firstName = "TestEvalUser")
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Disable FK constraints temporarily to insert without all dependent records
            await conn.ExecuteAsync(
                "ALTER TABLE [AppSecurity].[Users] NOCHECK CONSTRAINT ALL");

            const string sql = """
                INSERT INTO [AppSecurity].[Users]
                    (FirstName, EmailId, IsActive, IsDeleted, IsFirstTimeUser, DepartmentId, IsLocked,
                     CreatedBy, CreatedAt, CreatedByName, CreatedIP)
                VALUES
                    (@FirstName, 'test@test.com', 1, 0, 0, 0, 0, 0, GETUTCDATE(), 'test', '127.0.0.1');
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            var userId = await conn.ExecuteScalarAsync<int>(sql, new { FirstName = firstName });

            await conn.ExecuteAsync(
                "ALTER TABLE [AppSecurity].[Users] CHECK CONSTRAINT ALL");

            return userId;
        }

        private async Task<int> SeedRoleAsync(string roleName, bool bypassDataAccess = false)
        {
            await using var ctx = CreateDbContext();
            var existing = await ctx.UserRole.FirstOrDefaultAsync(
                r => r.RoleName == roleName && r.IsDeleted == IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var cmdRepo = new UserRoleCommandRepository(ctx);
            var role = new UserRole
            {
                RoleName         = roleName,
                Description      = "",
                CompanyId        = 1,
                BypassDataAccess = bypassDataAccess,
                IsActive         = Status.Active,
                IsDeleted        = IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(role);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task SeedAllocationAsync(int userId, int roleId, byte isActive = 1)
        {
            await using var ctx = CreateDbContext();

            // Disable FK to Users table so we don't need a real User row dependency
            await ctx.Database.ExecuteSqlRawAsync(
                "ALTER TABLE [AppSecurity].[UserRoleAllocation] NOCHECK CONSTRAINT ALL");

            await ctx.UserRoleAllocations.AddAsync(new UserRoleAllocationEntity
            {
                UserId     = userId,
                UserRoleId = roleId,
                IsActive   = isActive
            });
            await ctx.SaveChangesAsync();

            await ctx.Database.ExecuteSqlRawAsync(
                "ALTER TABLE [AppSecurity].[UserRoleAllocation] WITH CHECK CHECK CONSTRAINT ALL");
        }

        private async Task<int> SeedPolicyAsync(
            string code, string name = "Eval Policy",
            string entity = "SalesOrder", string field = "TypeId",
            bool active = true)
        {
            await using var ctx = CreateDbContext();
            var repo = new AccessPolicyCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.AccessPolicy
            {
                PolicyCode = code,
                PolicyName = name,
                EntityName = entity,
                FieldName  = field,
                IsActive   = active ? Status.Active : Status.Inactive,
                IsDeleted  = IsDelete.NotDeleted
            });
        }

        // --- CHECK BYPASS ---

        [Fact]
        public async Task CheckBypassAsync_Should_Return_True_When_Bypass_Role_Allocated()
        {
            await ClearAsync();
            var roleId = await SeedRoleAsync("TestRole_Bypass_True", bypassDataAccess: true);
            var userId = await SeedUserAsync("BypassUser");
            await SeedAllocationAsync(userId, roleId, isActive: 1);

            var result = await CreateEvalRepo().CheckBypassAsync(userId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckBypassAsync_Should_Return_False_When_Role_Has_No_Bypass()
        {
            await ClearAsync();
            var roleId = await SeedRoleAsync("TestRole_Bypass_False", bypassDataAccess: false);
            var userId = await SeedUserAsync("NonBypassUser");
            await SeedAllocationAsync(userId, roleId, isActive: 1);

            var result = await CreateEvalRepo().CheckBypassAsync(userId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckBypassAsync_Should_Return_False_When_Allocation_Is_Inactive()
        {
            await ClearAsync();
            var roleId = await SeedRoleAsync("TestRole_Bypass_Inactive", bypassDataAccess: true);
            var userId = await SeedUserAsync("InactiveAllocUser");
            await SeedAllocationAsync(userId, roleId, isActive: 0);

            var result = await CreateEvalRepo().CheckBypassAsync(userId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckBypassAsync_Should_Return_False_When_No_Allocation()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("NoAllocUser");

            var result = await CreateEvalRepo().CheckBypassAsync(userId);

            result.Should().BeFalse();
        }

        // --- GET USER ROLE IDs ---

        [Fact]
        public async Task GetUserRoleIdsAsync_Should_Return_Active_Role_Ids()
        {
            await ClearAsync();
            var roleId1 = await SeedRoleAsync("TestRole_Eval_R1");
            var roleId2 = await SeedRoleAsync("TestRole_Eval_R2");
            var userId  = await SeedUserAsync("RoleIdsUser");
            await SeedAllocationAsync(userId, roleId1, isActive: 1);
            await SeedAllocationAsync(userId, roleId2, isActive: 1);

            var roleIds = await CreateEvalRepo().GetUserRoleIdsAsync(userId);

            roleIds.Should().HaveCount(2);
            roleIds.Should().Contain(roleId1);
            roleIds.Should().Contain(roleId2);
        }

        [Fact]
        public async Task GetUserRoleIdsAsync_Should_Return_Empty_When_No_Allocations()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("NoRoleUser");

            var roleIds = await CreateEvalRepo().GetUserRoleIdsAsync(userId);

            roleIds.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserRoleIdsAsync_Should_Exclude_Inactive_Allocations()
        {
            await ClearAsync();
            var roleId = await SeedRoleAsync("TestRole_Eval_Inactive");
            var userId = await SeedUserAsync("InactiveRoleUser");
            await SeedAllocationAsync(userId, roleId, isActive: 0);

            var roleIds = await CreateEvalRepo().GetUserRoleIdsAsync(userId);

            roleIds.Should().BeEmpty();
        }

        // --- GET ALLOWED VALUE IDs ---

        [Fact]
        public async Task GetAllowedValueIdsAsync_Should_Return_Null_When_Policy_Not_Configured()
        {
            await ClearAsync();

            var result = await CreateEvalRepo().GetAllowedValueIdsAsync(
                "NONEXISTENT_POLICY", new[] { 1, 2 });

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllowedValueIdsAsync_Should_Return_Empty_When_No_Role_Ids()
        {
            await ClearAsync();
            await SeedPolicyAsync("EVALP01", active: true);

            var result = await CreateEvalRepo().GetAllowedValueIdsAsync(
                "EVALP01", Array.Empty<int>());

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllowedValueIdsAsync_Should_Return_Null_When_Policy_Inactive()
        {
            await ClearAsync();
            await SeedPolicyAsync("EVALP02", active: false);

            var result = await CreateEvalRepo().GetAllowedValueIdsAsync(
                "EVALP02", new[] { 1 });

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllowedValueIdsAsync_Should_Return_Allowed_Value_Ids()
        {
            await ClearAsync();
            var policyId = await SeedPolicyAsync("EVALP03");
            var roleId   = await SeedRoleAsync("TestRole_Eval_Allowed");

            await using var ctx = CreateDbContext();
            var cmdRepo = new AccessPolicyCommandRepository(ctx);
            await cmdRepo.AssignRoleValueAsync(new RoleAccessPolicy
                { AccessPolicyId = policyId, RoleId = roleId, ValueId = 10 });
            await cmdRepo.AssignRoleValueAsync(new RoleAccessPolicy
                { AccessPolicyId = policyId, RoleId = roleId, ValueId = 20 });

            var result = await CreateEvalRepo().GetAllowedValueIdsAsync(
                "EVALP03", new[] { roleId });

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(10);
            result.Should().Contain(20);
        }

        [Fact]
        public async Task GetAllowedValueIdsAsync_Should_Return_Empty_When_Role_Has_No_Assignment()
        {
            await ClearAsync();
            await SeedPolicyAsync("EVALP04");
            var roleId = await SeedRoleAsync("TestRole_Eval_NoAssign");

            // Policy exists but this role has no RoleAccessPolicy rows
            var result = await CreateEvalRepo().GetAllowedValueIdsAsync(
                "EVALP04", new[] { roleId });

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
