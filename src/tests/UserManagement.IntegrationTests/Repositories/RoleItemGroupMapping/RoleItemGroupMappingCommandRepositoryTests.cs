using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
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
    public sealed class RoleItemGroupMappingCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RoleItemGroupMappingCommandRepositoryTests(DbFixture fixture)
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

        private RoleItemGroupMappingCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new RoleItemGroupMappingCommandRepository(ctx);

        private async Task<int> EnsureRoleAsync(ApplicationDbContext ctx, string roleName = "TestRole_RIGM")
        {
            var existing = await ctx.UserRole.FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var cmdRepo = new UserRoleCommandRepository(ctx);
            var role = new UserRole
            {
                RoleName = roleName,
                Description = "Test Role for RIGM",
                CompanyId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(role);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx, int roleId) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Create");

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await repo.CreateAsync(entity);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Persist");

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 5,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RoleItemGroupMapping.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.RoleId.Should().Be(roleId);
            saved.ItemGroupId.Should().Be(5);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Update");

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var created = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 2,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(created.Id, updateEntity);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.RoleItemGroupMapping.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.ItemGroupId.Should().Be(2);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_Delete");

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var created = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.RoleItemGroupMapping { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.RoleItemGroupMapping.FirstOrDefaultAsync(x => x.Id == created.Id);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.DeleteAsync(99999, new Domain.Entities.RoleItemGroupMapping { IsDeleted = Enums.IsDelete.Deleted });

            result.Should().Be(0);
        }

        // --- COMPOSITE KEY EXISTS ---

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_When_Mapping_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx, 0);
            var roleId = await EnsureRoleAsync(ctx, "TestRole_RIGM_CompositeKey");

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.RoleItemGroupMapping
            {
                RoleId = roleId,
                ItemGroupId = 10,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await repo.CreateAsync(entity);

            await using var ctx2 = CreateDbContext();
            var repo2 = new RoleItemGroupMappingCommandRepository(ctx2);
            var exists = await repo2.CompositeKeyExistsAsync(roleId, 10);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var exists = await repo.CompositeKeyExistsAsync(99999, 99999);

            exists.Should().BeFalse();
        }
    }
}
