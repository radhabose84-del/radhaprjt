using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
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
    public sealed class AccessPolicyQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AccessPolicyQueryRepositoryTests(DbFixture fixture)
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

        private AccessPolicyQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedPolicyAsync(
            string code = "QAP001",
            string name = "Query Policy",
            string entity = "SalesOrder",
            string field  = "SalesOrderTypeId")
        {
            await using var ctx = CreateDbContext();
            var repo = new AccessPolicyCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.AccessPolicy
            {
                PolicyCode = code,
                PolicyName = name,
                EntityName = entity,
                FieldName  = field,
                IsActive   = Status.Active,
                IsDeleted  = IsDelete.NotDeleted
            });
        }

        private async Task<int> EnsureRoleAsync(string roleName = "TestRole_AQ")
        {
            await using var ctx = CreateDbContext();
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

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedPolicyAsync("QAP010", "Alpha Policy");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].PolicyCode.Should().Be("QAP010");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAP011");
            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedPolicyAsync("QAPALPHA", "Alpha Policy");
            await SeedPolicyAsync("QAPBETA",  "Beta Policy");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].PolicyName.Should().Be("Alpha Policy");
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Paginate_Correctly()
        {
            await ClearAsync();
            await SeedPolicyAsync("QAPP01", "Policy One");
            await SeedPolicyAsync("QAPP02", "Policy Two");
            await SeedPolicyAsync("QAPP03", "Policy Three");

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 2, null);

            page1.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPID01", "Id Policy", "PurchaseOrder", "VendorId");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.PolicyCode.Should().Be("QAPID01");
            dto.PolicyName.Should().Be("Id Policy");
            dto.EntityName.Should().Be("PurchaseOrder");
            dto.FieldName.Should().Be("VendorId");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_Nonexistent()
        {
            await ClearAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPDEL01");
            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await ClearAsync();
            await SeedPolicyAsync("QAPAUTO1", "AutoOrder Policy");
            await SeedPolicyAsync("QAPOTHER", "Something Else");

            var results = await CreateQueryRepo().AutocompleteAsync("Auto", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].PolicyName.Should().Be("AutoOrder Policy");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPINACT", "Inactive Policy");
            await using var ctx = CreateDbContext();
            var entity = await ctx.AccessPolicies.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Inactive", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPDEL2", "Deleted Policy");
            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate_Code()
        {
            await ClearAsync();
            await SeedPolicyAsync("QAPDUP01");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("QAPDUP01");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_New_Code()
        {
            await ClearAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("QAPNEW99");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted_Code()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPSD01");
            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("QAPSD01");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_ExcludeId_Provided()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPEXCL1");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("QAPEXCL1", id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Record_Exists()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPNF01");

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Record_Missing()
        {
            await ClearAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedPolicyAsync("QAPNFSD1");
            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // --- GET ROLE ACCESS POLICIES ---

        [Fact]
        public async Task GetRoleAccessPoliciesAsync_Should_Return_Assigned_Policies()
        {
            await ClearAsync();
            var policyId = await SeedPolicyAsync("QAPRAP01");
            var roleId   = await EnsureRoleAsync("TestRole_AQ_RAP");

            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 7
            });

            var results = await CreateQueryRepo().GetRoleAccessPoliciesAsync(policyId, null);

            results.Should().HaveCount(1);
            results[0].RoleId.Should().Be(roleId);
            results[0].ValueId.Should().Be(7);
        }

        [Fact]
        public async Task GetRoleAccessPoliciesAsync_Should_Filter_By_RoleId()
        {
            await ClearAsync();
            var policyId = await SeedPolicyAsync("QAPRAP02");
            var roleId1  = await EnsureRoleAsync("TestRole_AQ_R1");
            var roleId2  = await EnsureRoleAsync("TestRole_AQ_R2");

            await using var ctx = CreateDbContext();
            var cmdRepo = new AccessPolicyCommandRepository(ctx);
            await cmdRepo.AssignRoleValueAsync(new RoleAccessPolicy { AccessPolicyId = policyId, RoleId = roleId1, ValueId = 1 });
            await cmdRepo.AssignRoleValueAsync(new RoleAccessPolicy { AccessPolicyId = policyId, RoleId = roleId2, ValueId = 2 });

            var results = await CreateQueryRepo().GetRoleAccessPoliciesAsync(policyId, roleId1);

            results.Should().HaveCount(1);
            results[0].RoleId.Should().Be(roleId1);
        }

        // --- ROLE VALUE ASSIGNMENT EXISTS ---

        [Fact]
        public async Task RoleValueAssignmentExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearAsync();
            var policyId = await SeedPolicyAsync("QAPRVA01");
            var roleId   = await EnsureRoleAsync("TestRole_AQ_RVA");

            await using var ctx = CreateDbContext();
            await new AccessPolicyCommandRepository(ctx).AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 3
            });

            var exists = await CreateQueryRepo().RoleValueAssignmentExistsAsync(policyId, roleId, 3);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task RoleValueAssignmentExistsAsync_Should_Return_False_When_Not_Assigned()
        {
            await ClearAsync();
            var policyId = await SeedPolicyAsync("QAPRVA02");
            var roleId   = await EnsureRoleAsync("TestRole_AQ_RVAF");

            var exists = await CreateQueryRepo().RoleValueAssignmentExistsAsync(policyId, roleId, 99);

            exists.Should().BeFalse();
        }

        // --- USER ROLE EXISTS ---

        [Fact]
        public async Task UserRoleExistsAsync_Should_Return_True_For_Active_Role()
        {
            await ClearAsync();
            var roleId = await EnsureRoleAsync("TestRole_AQ_URE");

            var exists = await CreateQueryRepo().UserRoleExistsAsync(roleId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task UserRoleExistsAsync_Should_Return_False_For_NonExistent_Role()
        {
            await ClearAsync();

            var exists = await CreateQueryRepo().UserRoleExistsAsync(9999);

            exists.Should().BeFalse();
        }

        // --- ROLE ACCESS POLICY NOT FOUND ---

        [Fact]
        public async Task RoleAccessPolicyNotFoundAsync_Should_Return_False_When_Assignment_Exists()
        {
            await ClearAsync();
            var policyId = await SeedPolicyAsync("QAPRPNF1");
            var roleId   = await EnsureRoleAsync("TestRole_AQ_NF");

            await using var ctx = CreateDbContext();
            var assignId = await new AccessPolicyCommandRepository(ctx).AssignRoleValueAsync(new RoleAccessPolicy
            {
                AccessPolicyId = policyId,
                RoleId         = roleId,
                ValueId        = 4
            });

            var notFound = await CreateQueryRepo().RoleAccessPolicyNotFoundAsync(assignId);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task RoleAccessPolicyNotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearAsync();

            var notFound = await CreateQueryRepo().RoleAccessPolicyNotFoundAsync(9999);

            notFound.Should().BeTrue();
        }
    }
}
