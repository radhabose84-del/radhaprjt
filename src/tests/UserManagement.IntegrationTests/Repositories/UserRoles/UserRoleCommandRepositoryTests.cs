using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.UserRoles;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserRoles
{
    [Collection("DatabaseCollection")]
    public sealed class UserRoleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserRoleCommandRepositoryTests(DbFixture fixture)
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

        private UserRoleCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static UserRole BuildUserRole(
            string roleName = "TestRole_URC",
            string description = "Test Role Description",
            int companyId = 1) =>
            new UserRole
            {
                RoleName = roleName,
                Description = description,
                CompanyId = companyId,
                BypassDataAccess = false,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTestRolesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var role = BuildUserRole("TestRole_URC_Create");

            var result = await repo.CreateAsync(role);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var role = BuildUserRole("TestRole_URC_Persist", "Persist Description", 1);
            var created = await repo.CreateAsync(role);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserRole.FirstOrDefaultAsync(r => r.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.RoleName.Should().Be("TestRole_URC_Persist");
            saved.Description.Should().Be("Persist Description");
            saved.CompanyId.Should().Be(1);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserRole("TestRole_URC_Audit"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserRole.FirstOrDefaultAsync(r => r.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserRole("TestRole_URC_Update", "Original"));
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildUserRole("TestRole_URC_Update", "Updated Description");
            updateEntity.BypassDataAccess = true;

            var result = await repo.UpdateAsync(created.Id, updateEntity);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.UserRole.FirstOrDefaultAsync(r => r.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Updated Description");
            updated.BypassDataAccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.UpdateAsync(999999, BuildUserRole("TestRole_URC_NotFound"));

            result.Should().Be(0);
        }

        // --- DELETE (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserRole("TestRole_URC_Delete"));
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserRole { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.UserRole.FirstOrDefaultAsync(r => r.Id == created.Id);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.DeleteAsync(999999, new UserRole { IsDeleted = Enums.IsDelete.Deleted });

            result.Should().Be(0);
        }

        // --- ExistsByCodeAsync / ExistsByNameupdateAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_RoleName_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildUserRole("TestRole_URC_Exists"));
            ctx.ChangeTracker.Clear();

            var exists = await repo.ExistsByCodeAsync("TestRole_URC_Exists");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_Not_Found()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var exists = await repo.ExistsByCodeAsync("TestRole_URC_NonExistent_XYZ");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByNameupdateAsync_Should_Exclude_Self()
        {
            await using var ctx = CreateDbContext();
            await ClearTestRolesAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserRole("TestRole_URC_UpdateExists"));
            ctx.ChangeTracker.Clear();

            // Excluding the current record's Id → should be false
            var exists = await repo.ExistsByNameupdateAsync("TestRole_URC_UpdateExists", created.Id);

            exists.Should().BeFalse();
        }
    }
}
