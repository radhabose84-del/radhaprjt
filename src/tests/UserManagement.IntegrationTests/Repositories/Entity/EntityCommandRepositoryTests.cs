using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Entities;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Entity
{
    [Collection("DatabaseCollection")]
    public sealed class EntityCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EntityCommandRepositoryTests(DbFixture fixture)
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

        private EntityCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new EntityCommandRepository(ctx);

        private static Domain.Entities.Entity BuildEntity(
            string code = "ENT-TST01",
            string name = "Test Entity CMD") =>
            new Domain.Entities.Entity
            {
                EntityCode = code,
                EntityName = name,
                EntityDescription = "Test Description",
                Address = "Test Address",
                Phone = "1234567890",
                Email = "test@test.com",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppData.Entity WHERE EntityCode LIKE 'ENT-TST%' OR EntityName LIKE 'Test Entity CMD%'");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity("ENT-TST02", "Test Entity CMD Alpha"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Entity.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.EntityCode.Should().Be("ENT-TST02");
            saved.EntityName.Should().Be("Test Entity CMD Alpha");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Entity.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity("ENT-TST03", "Original Name"));
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.Entity
            {
                EntityName = "Updated Entity Name",
                EntityDescription = "Updated Desc",
                Address = "Updated Address",
                Phone = "9876543210",
                Email = "updated@test.com",
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(newId, updateEntity);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.Entity.FirstOrDefaultAsync(x => x.Id == newId);

            updated.Should().NotBeNull();
            updated!.EntityName.Should().Be("Updated Entity Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(99999, BuildEntity());

            result.Should().Be(-1);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteEntityAsync_Should_Soft_Delete()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity("ENT-TST04", "Delete Entity"));
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Entity
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteEntityAsync(newId, deleteModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.Entity.FirstOrDefaultAsync(x => x.Id == newId);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteEntityAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.DeleteEntityAsync(99999, new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted });

            result.Should().Be(-1);
        }

        // --- EXISTS BY CODE ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_For_Existing_Name()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildEntity("ENT-TST05", "Exists Test Entity"));

            var exists = await repo.ExistsByCodeAsync("Exists Test Entity");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var exists = await repo.ExistsByCodeAsync("NonExistentEntityName_XYZ999");

            exists.Should().BeFalse();
        }
    }
}
