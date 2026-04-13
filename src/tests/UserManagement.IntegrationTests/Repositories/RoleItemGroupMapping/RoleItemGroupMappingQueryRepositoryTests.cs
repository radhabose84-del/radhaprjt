using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.RoleItemGroupMapping;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.RoleItemGroupMapping
{
    [Collection("DatabaseCollection")]
    public sealed class RoleItemGroupMappingQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RoleItemGroupMappingQueryRepositoryTests(DbFixture fixture)
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

        private RoleItemGroupMappingQueryRepository CreateQueryRepo(ApplicationDbContext ctx)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new RoleItemGroupMappingQueryRepository(ctx, conn);
        }

        private async Task<int> EnsureRoleAsync(ApplicationDbContext ctx, string roleName = "TestRole_RIGM_Q")
        {
            var existing = await ctx.UserRole.FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var cmdRepo = new UserRoleCommandRepository(ctx);
            var role = new UserRole
            {
                RoleName = roleName,
                Description = "Test Role for RIGM Query",
                CompanyId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(role);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task<int> SeedMappingAsync(ApplicationDbContext ctx, int roleId, int itemGroupId)
        {
            var cmdRepo = new RoleItemGroupMappingCommandRepository(ctx);
            var created = await cmdRepo.CreateAsync(new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = itemGroupId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();
            return created.Id;
        }

        private async Task ClearMappingsAsync(ApplicationDbContext ctx, int roleId)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                $"DELETE FROM AppSecurity.RoleItemGroupMapping WHERE RoleId = {roleId}");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_GetById");
            await ClearMappingsAsync(ctx, roleId);
            var id = await SeedMappingAsync(ctx, roleId, itemGroupId: 5);

            await using var queryCtx = CreateDbContext();
            var result = await CreateQueryRepo(queryCtx).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.RoleId.Should().Be(roleId);
            result.ItemGroupId.Should().Be(5);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var result = await CreateQueryRepo(ctx).GetByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_SoftDel");
            await ClearMappingsAsync(ctx, roleId);
            var id = await SeedMappingAsync(ctx, roleId, itemGroupId: 1);

            var cmdRepo = new RoleItemGroupMappingCommandRepository(ctx);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.RoleItemGroupMapping
            {
                IsDeleted = Enums.IsDelete.Deleted
            });
            ctx.ChangeTracker.Clear();

            await using var queryCtx = CreateDbContext();
            var result = await CreateQueryRepo(queryCtx).GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Paginated_Results()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_GetAll");
            await ClearMappingsAsync(ctx, roleId);
            await SeedMappingAsync(ctx, roleId, itemGroupId: 1);
            await SeedMappingAsync(ctx, roleId, itemGroupId: 2);

            await using var queryCtx = CreateDbContext();
            var (items, total) = await CreateQueryRepo(queryCtx).GetAllAsync(1, 10, null);

            items.Should().NotBeNull();
            // At least the two we seeded should be present
            items.Count.Should().BeGreaterThanOrEqualTo(2);
            total.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_Exclude");
            await ClearMappingsAsync(ctx, roleId);
            var id = await SeedMappingAsync(ctx, roleId, itemGroupId: 3);

            var cmdRepo = new RoleItemGroupMappingCommandRepository(ctx);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.RoleItemGroupMapping
            {
                IsDeleted = Enums.IsDelete.Deleted
            });
            ctx.ChangeTracker.Clear();

            await using var queryCtx = CreateDbContext();
            var (items, _) = await CreateQueryRepo(queryCtx).GetAllAsync(1, 100, null);

            items.Should().NotContain(x => x.Id == id);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_Search_Uniq");
            await ClearMappingsAsync(ctx, roleId);
            await SeedMappingAsync(ctx, roleId, itemGroupId: 10);

            await using var queryCtx = CreateDbContext();
            var (items, _) = await CreateQueryRepo(queryCtx).GetAllAsync(1, 10, "TestRole_RIGM_Q_Search_Uniq");

            items.Should().NotBeEmpty();
            items.Should().OnlyContain(x => x.RoleId == roleId);
        }

        // --- GET BY ROLE ID ---

        [Fact]
        public async Task GetByRoleIdAsync_Should_Return_Active_Mappings()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_ByRole");
            await ClearMappingsAsync(ctx, roleId);
            await SeedMappingAsync(ctx, roleId, itemGroupId: 1);
            await SeedMappingAsync(ctx, roleId, itemGroupId: 2);

            await using var queryCtx = CreateDbContext();
            var results = await CreateQueryRepo(queryCtx).GetByRoleIdAsync(roleId);

            results.Should().HaveCount(2);
            results.Should().OnlyContain(x => x.RoleId == roleId);
            results.Should().BeInAscendingOrder(x => x.ItemGroupId);
        }

        [Fact]
        public async Task GetByRoleIdAsync_Should_Exclude_Inactive()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_Inactive");
            await ClearMappingsAsync(ctx, roleId);

            var cmdRepo = new RoleItemGroupMappingCommandRepository(ctx);
            await cmdRepo.CreateAsync(new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 5,
                IsActive = Enums.Status.Inactive,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            await using var queryCtx = CreateDbContext();
            var results = await CreateQueryRepo(queryCtx).GetByRoleIdAsync(roleId);

            results.Should().BeEmpty();
        }

        // --- NOT FOUND ASYNC ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Entity_Exists()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Q_NotFound");
            await ClearMappingsAsync(ctx, roleId);
            var id = await SeedMappingAsync(ctx, roleId, itemGroupId: 1);

            await using var queryCtx = CreateDbContext();
            var notFound = await CreateQueryRepo(queryCtx).NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var notFound = await CreateQueryRepo(ctx).NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_Always()
        {
            await using var ctx = CreateDbContext();
            var result = await CreateQueryRepo(ctx).SoftDeleteValidation(1);

            result.Should().BeFalse();
        }
    }
}
