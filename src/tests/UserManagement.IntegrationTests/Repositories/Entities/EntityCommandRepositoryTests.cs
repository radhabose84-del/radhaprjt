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

namespace UserManagement.IntegrationTests.Repositories.Entities
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

        private EntityCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.Entity BuildEntity(
            string code = "ENT-00001",
            string name = "Test Entity",
            string description = "Test Description",
            string address = "Test Address",
            string phone = "9999999999",
            string email = "test@example.com") =>
            new Domain.Entities.Entity
            {
                EntityCode = code,
                EntityName = name,
                EntityDescription = description,
                Address = address,
                Phone = phone,
                Email = email,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("ENT-00002", "Persisted Entity"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Entity.FirstOrDefaultAsync(e => e.Id == id);

            saved.Should().NotBeNull();
            saved!.EntityCode.Should().Be("ENT-00002");
            saved.EntityName.Should().Be("Persisted Entity");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Entity.FirstOrDefaultAsync(e => e.Id == id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("ENT-00003", "Original Name"));
            ctx.ChangeTracker.Clear();

            var update = BuildEntity("ENT-00003", "Updated Name");
            var result = await repo.UpdateAsync(id, update);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Entity.FirstOrDefaultAsync(e => e.Id == id);
            saved.Should().NotBeNull();
            saved!.EntityName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Minus1_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(9999, BuildEntity());

            result.Should().Be(-1);
        }

        [Fact]
        public async Task DeleteEntityAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteEntityAsync(id, deleteModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Entity.FirstOrDefaultAsync(e => e.Id == id);
            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteEntityAsync_Should_Return_Minus1_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var deleteModel = new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted };

            var result = await repo.DeleteEntityAsync(9999, deleteModel);

            result.Should().Be(-1);
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Name_Present()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildEntity("ENT-00004", "Unique Entity"));
            ctx.ChangeTracker.Clear();

            var exists = await repo.ExistsByCodeAsync("Unique Entity");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task GetByNameAsync_Should_Return_Entity_When_Present()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildEntity("ENT-00005", "Findable Entity"));
            ctx.ChangeTracker.Clear();

            var result = await repo.GetByNameAsync("Findable Entity");

            result.Should().NotBeNull();
            result!.EntityName.Should().Be("Findable Entity");
        }

        [Fact]
        public async Task ExistsByNameupdateAsync_Should_Return_False_When_Only_Self_Matches()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("ENT-00006", "Self Entity"));
            ctx.ChangeTracker.Clear();

            var result = await repo.ExistsByNameupdateAsync("Self Entity", id);

            result.Should().BeFalse();
        }
    }
}
