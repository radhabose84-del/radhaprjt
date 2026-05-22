using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using FluentAssertions;
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
    public sealed class AccessPolicyCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AccessPolicyCommandRepositoryTests(DbFixture fixture)
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

        private static AccessPolicyCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.AccessPolicy BuildAccessPolicy(
            string code = "AP001",
            string name = "Test Policy",
            string entity = "SalesOrder",
            string field = "SalesOrderTypeId") =>
            new()
            {
                PolicyCode = code,
                PolicyName = name,
                EntityName = entity,
                FieldName  = field,
                IsActive   = Status.Active,
                IsDeleted  = IsDelete.NotDeleted
            };

        private async Task<int> EnsureRoleAsync(ApplicationDbContext ctx, string roleName = "TestRole_AP")
        {
            var existing = await ctx.UserRole.FirstOrDefaultAsync(
                r => r.RoleName == roleName && r.IsDeleted == IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var cmdRepo = new UserRoleCommandRepository(ctx);
            var role = new UserRole
            {
                RoleName    = roleName,
                Description = "",
                CompanyId   = 1,
                IsActive    = Status.Active,
                IsDeleted   = IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(role);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildAccessPolicy());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo  = CreateRepository(ctx);

            var newId = await repo.CreateAsync(BuildAccessPolicy("AP002", "Order Policy", "SalesOrder", "StatusId"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AccessPolicies.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PolicyCode.Should().Be("AP002");
            saved.PolicyName.Should().Be("Order Policy");
            saved.EntityName.Should().Be("SalesOrder");
            saved.FieldName.Should().Be("StatusId");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildAccessPolicy("AP003"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AccessPolicies.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedAt.Should().BeAfter(DateTime.MinValue);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var id = await repo.CreateAsync(BuildAccessPolicy("AP010", "Original Name"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.AccessPolicies.FirstAsync(x => x.Id == id);
            entity.PolicyName = "Updated Name";
            entity.EntityName = "PurchaseOrder";
            entity.FieldName  = "VendorId";

            var result = await repo.UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.AccessPolicies.FirstAsync(x => x.Id == id);
            updated.PolicyName.Should().Be("Updated Name");
            updated.EntityName.Should().Be("PurchaseOrder");
            updated.FieldName.Should().Be("VendorId");
            result.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();

            var ghost = new Domain.Entities.AccessPolicy { Id = 9999, PolicyName = "Ghost" };
            var result = await CreateRepository(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_PolicyCode()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var id = await repo.CreateAsync(BuildAccessPolicy("AP011", "OriginalName"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.AccessPolicies.FirstAsync(x => x.Id == id);
            entity.PolicyName = "Changed Name";
            await repo.UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.AccessPolicies.FirstAsync(x => x.Id == id);
            updated.PolicyCode.Should().Be("AP011");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var id = await repo.CreateAsync(BuildAccessPolicy("AP020"));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var id = await repo.CreateAsync(BuildAccessPolicy("AP021"));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AccessPolicies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- ASSIGN ROLE VALUE ---

        [Fact]
        public async Task AssignRoleValueAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var policyId = await repo.CreateAsync(BuildAccessPolicy("AP030"));
            var roleId   = await EnsureRoleAsync(ctx, "TestRole_AP_Assign");
            ctx.ChangeTracker.Clear();

            var newId = await repo.AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 1
            });

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AssignRoleValueAsync_Should_Persist_Record()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var policyId = await repo.CreateAsync(BuildAccessPolicy("AP031"));
            var roleId   = await EnsureRoleAsync(ctx, "TestRole_AP_AssignPersist");
            ctx.ChangeTracker.Clear();

            var newId = await repo.AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 5
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RoleAccessPolicies.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.AccessPolicyId.Should().Be(policyId);
            saved.RoleId.Should().Be(roleId);
            saved.ValueId.Should().Be(5);
        }

        // --- REMOVE ROLE VALUE ---

        [Fact]
        public async Task RemoveRoleValueAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var policyId = await repo.CreateAsync(BuildAccessPolicy("AP040"));
            var roleId   = await EnsureRoleAsync(ctx, "TestRole_AP_Remove");
            ctx.ChangeTracker.Clear();

            var assignId = await repo.AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 10
            });
            ctx.ChangeTracker.Clear();

            var result = await repo.RemoveRoleValueAsync(assignId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveRoleValueAsync_Should_Physically_Remove_Record()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var policyId = await repo.CreateAsync(BuildAccessPolicy("AP041"));
            var roleId   = await EnsureRoleAsync(ctx, "TestRole_AP_RemovePhys");
            ctx.ChangeTracker.Clear();

            var assignId = await repo.AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 11
            });
            ctx.ChangeTracker.Clear();

            await repo.RemoveRoleValueAsync(assignId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var removed = await ctx.RoleAccessPolicies.FirstOrDefaultAsync(x => x.Id == assignId);
            removed.Should().BeNull();
        }

        [Fact]
        public async Task RemoveRoleValueAsync_Should_Return_False_When_NotFound()
        {
            await ClearAsync();
            await using var ctx = CreateDbContext();

            var result = await CreateRepository(ctx).RemoveRoleValueAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
