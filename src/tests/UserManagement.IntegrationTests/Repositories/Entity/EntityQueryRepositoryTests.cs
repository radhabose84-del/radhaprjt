using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Entities;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Entity
{
    [Collection("DatabaseCollection")]
    public sealed class EntityQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EntityQueryRepositoryTests(DbFixture fixture)
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

        private EntityQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new EntityQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedAsync(
            ApplicationDbContext ctx,
            string code = "ENT-QRY01",
            string name = "Query Entity Test")
        {
            var cmdRepo = new EntityCommandRepository(ctx);
            var entity = new Domain.Entities.Entity
            {
                EntityCode = code,
                EntityName = name,
                EntityDescription = "Test",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var newId = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return newId;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppData.Entity WHERE EntityCode LIKE 'ENT-QRY%'");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllEntityAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "ENT-QRY01", "Query Entity Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllEntityAsync(1, 100, null);

            items.Should().Contain(e => e.EntityCode == "ENT-QRY01");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllEntityAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "ENT-QRY02", "Searchable Entity Test");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllEntityAsync(1, 10, "ENT-QRY02");

            items.Should().HaveCount(1);
            items[0].EntityCode.Should().Be("ENT-QRY02");
        }

        [Fact]
        public async Task GetAllEntityAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "ENT-QRY03", "Deleted Entity Test");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new EntityCommandRepository(ctx2);
            await cmdRepo.DeleteEntityAsync(id, new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllEntityAsync(1, 100, "ENT-QRY03");

            items.Should().NotContain(e => e.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "ENT-QRY04", "ById Entity Test");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.EntityCode.Should().Be("ENT-QRY04");
            result.EntityName.Should().Be("ById Entity Test");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "ENT-QRY05", "Soft Deleted Entity Test");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new EntityCommandRepository(ctx2);
            await cmdRepo.DeleteEntityAsync(id, new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
