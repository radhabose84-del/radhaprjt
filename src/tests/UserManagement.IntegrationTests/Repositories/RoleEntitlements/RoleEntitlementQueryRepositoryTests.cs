using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.RoleEntitlements;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.RoleEntitlements
{
    [Collection("DatabaseCollection")]
    public sealed class RoleEntitlementQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RoleEntitlementQueryRepositoryTests(DbFixture fixture)
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

        private RoleEntitlementQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new RoleEntitlementQueryRepository(conn);
        }

        private async Task<int> EnsureRoleAsync(ApplicationDbContext ctx, string roleName = "TestRole_REQ")
        {
            var existing = await ctx.UserRole.FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var cmdRepo = new UserRoleCommandRepository(ctx);
            var role = new UserRole
            {
                RoleName = roleName,
                Description = "Test Role for RoleEntitlement Query",
                CompanyId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(role);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Role_When_Exists()
        {
            await using var ctx = CreateDbContext();
            var roleId = await EnsureRoleAsync(ctx, "TestRole_REQ_GetById");

            var repo = CreateQueryRepo();
            var (role, modules, parents, children, privileges) = await repo.GetByIdAsync(roleId);

            role.Should().NotBeNull();
            role!.Id.Should().Be(roleId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_Role_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var (role, modules, parents, children, privileges) = await repo.GetByIdAsync(99999);

            role.Should().BeNull();
        }

        // --- GET ROLE BY NAME ---

        [Fact]
        public async Task GetRoleByNameAsync_Should_Return_Role_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await EnsureRoleAsync(ctx, "TestRole_REQ_ByName");

            var repo = CreateQueryRepo();
            var result = await repo.GetRoleByNameAsync("TestRole_REQ_ByName", CancellationToken.None);

            result.Should().NotBeNull();
            result!.RoleName.Should().Be("TestRole_REQ_ByName");
        }

        [Fact]
        public async Task GetRoleByNameAsync_Should_Return_Empty_Role_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetRoleByNameAsync("NonExistentRole_XYZ999", CancellationToken.None);

            // Returns new UserRole() when not found (per implementation)
            result.Should().NotBeNull();
        }
    }
}
