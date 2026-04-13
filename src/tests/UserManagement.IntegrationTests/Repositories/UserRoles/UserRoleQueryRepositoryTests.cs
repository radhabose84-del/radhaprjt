using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserRoles
{
    [Collection("DatabaseCollection")]
    public sealed class UserRoleQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserRoleQueryRepositoryTests(DbFixture fixture)
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

        private UserRoleQueryRepository CreateQueryRepo(int companyId = 1, int userId = 1)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(companyId);
            ipMock.Setup(x => x.GetUserId()).Returns(userId);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UserRoleQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedUserRoleAsync(
            string roleName = "TestRole_URQ",
            string description = "Test Query Role",
            int companyId = 1)
        {
            await using var ctx = CreateDbContext();
            var repo = new UserRoleCommandRepository(ctx);
            var created = await repo.CreateAsync(new UserRole
            {
                RoleName = roleName,
                Description = description,
                CompanyId = companyId,
                BypassDataAccess = false,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task ClearTestRolesAsync()
        {
            await using var ctx = CreateDbContext();
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppSecurity.UserRoleAllocation WHERE UserRoleId IN (SELECT Id FROM AppSecurity.UserRole WHERE RoleName LIKE 'TestRole_URQ%')");
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppSecurity.UserRole WHERE RoleName LIKE 'TestRole_URQ%'");
        }

        // --- GET ALL ROLES ---

        [Fact]
        public async Task GetAllRoleAsync_Should_Return_Seeded_Record()
        {
            await ClearTestRolesAsync();
            await SeedUserRoleAsync("TestRole_URQ_GetAll");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllRoleAsync(1, 100, null);

            items.Should().Contain(r => r.RoleName == "TestRole_URQ_GetAll");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllRoleAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTestRolesAsync();
            await SeedUserRoleAsync("TestRole_URQ_Alpha");
            await SeedUserRoleAsync("TestRole_URQ_Beta");

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllRoleAsync(1, 100, "TestRole_URQ_Alpha");

            items.Should().Contain(r => r.RoleName == "TestRole_URQ_Alpha");
            items.Should().NotContain(r => r.RoleName == "TestRole_URQ_Beta");
        }

        [Fact]
        public async Task GetAllRoleAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTestRolesAsync();
            var id = await SeedUserRoleAsync("TestRole_URQ_Deleted");

            await using (var ctx = CreateDbContext())
            {
                var cmdRepo = new UserRoleCommandRepository(ctx);
                await cmdRepo.DeleteAsync(id, new UserRole { IsDeleted = Enums.IsDelete.Deleted });
            }

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllRoleAsync(1, 100, "TestRole_URQ_Deleted");

            items.Should().NotContain(r => r.RoleName == "TestRole_URQ_Deleted");
        }

        [Fact]
        public async Task GetAllRoleAsync_Should_Filter_By_CompanyId()
        {
            await ClearTestRolesAsync();
            await SeedUserRoleAsync("TestRole_URQ_CoA", companyId: 1);
            await SeedUserRoleAsync("TestRole_URQ_CoB", companyId: 999);

            var repo = CreateQueryRepo(companyId: 1);
            var (items, _) = await repo.GetAllRoleAsync(1, 100, "TestRole_URQ_Co");

            items.Should().Contain(r => r.RoleName == "TestRole_URQ_CoA");
            items.Should().NotContain(r => r.RoleName == "TestRole_URQ_CoB");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Record()
        {
            await ClearTestRolesAsync();
            var id = await SeedUserRoleAsync("TestRole_URQ_ById", "By Id Description");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.RoleName.Should().Be("TestRole_URQ_ById");
            result.Description.Should().Be("By Id Description");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTestRolesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTestRolesAsync();
            var id = await SeedUserRoleAsync("TestRole_URQ_ByIdSoftDel");

            await using (var ctx = CreateDbContext())
            {
                var cmdRepo = new UserRoleCommandRepository(ctx);
                await cmdRepo.DeleteAsync(id, new UserRole { IsDeleted = Enums.IsDelete.Deleted });
            }

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetRoles_SuperAdmin (no allocation join) ---

        [Fact]
        public async Task GetRoles_SuperAdmin_Should_Return_Matching_Records()
        {
            await ClearTestRolesAsync();
            await SeedUserRoleAsync("TestRole_URQ_SA1");
            await SeedUserRoleAsync("TestRole_URQ_SA2");

            var repo = CreateQueryRepo();
            var results = await repo.GetRoles_SuperAdmin("TestRole_URQ_SA");

            results.Should().HaveCountGreaterThanOrEqualTo(2);
            results.Should().Contain(r => r.RoleName == "TestRole_URQ_SA1");
            results.Should().Contain(r => r.RoleName == "TestRole_URQ_SA2");
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Role()
        {
            await ClearTestRolesAsync();
            var id = await SeedUserRoleAsync("TestRole_URQ_FKValid");

            var repo = CreateQueryRepo();
            var exists = await repo.FKColumnExistValidation(id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_False_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var exists = await repo.FKColumnExistValidation(999999);

            exists.Should().BeFalse();
        }
    }
}
