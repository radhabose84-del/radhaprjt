using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Entities;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Entities
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

        private EntityQueryRepository CreateQueryRepo(int entityId = 1)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(entityId);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new EntityQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedEntityAsync(
            string code = "ENT-00001",
            string name = "Test Entity")
        {
            await using var ctx = CreateDbContext();
            var repo = new EntityCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Entity
            {
                EntityCode = code,
                EntityName = name,
                EntityDescription = "Description",
                Address = "Address",
                Phone = "9999999999",
                Email = "test@example.com",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var ctx = CreateDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Entity");
        }

        // --- GenerateEntityCode ---

        [Fact]
        public async Task GenerateEntityCodeAsync_Should_Return_Initial_Code_When_Empty()
        {
            await ClearTableAsync();

            var code = await CreateQueryRepo().GenerateEntityCodeAsync();

            code.Should().Be("ENT-00001");
        }

        [Fact]
        public async Task GenerateEntityCodeAsync_Should_Increment_Last_Code()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ENT-00005", "Existing");

            var code = await CreateQueryRepo().GenerateEntityCodeAsync();

            code.Should().Be("ENT-00006");
        }

        // --- GetAllEntityAsync ---

        [Fact]
        public async Task GetAllEntityAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllEntityAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllEntityAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted };
            await new EntityCommandRepository(ctx).DeleteEntityAsync(id, deleteModel);

            var (items, total) = await CreateQueryRepo().GetAllEntityAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllEntityAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ENT-00001", "Alpha Entity");
            await SeedEntityAsync("ENT-00002", "Beta Entity");

            var (items, _) = await CreateQueryRepo().GetAllEntityAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].EntityName.Should().Be("Alpha Entity");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("ENT-00010", "Findable");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.EntityCode.Should().Be("ENT-00010");
            result.EntityName.Should().Be("Findable");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Entity { IsDeleted = Enums.IsDelete.Deleted };
            await new EntityCommandRepository(ctx).DeleteEntityAsync(id, deleteModel);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- GetByEntityName_SuperAdmin ---

        [Fact]
        public async Task GetByEntityName_SuperAdmin_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ENT-00001", "Alpha");
            await SeedEntityAsync("ENT-00002", "Beta");

            var result = await CreateQueryRepo().GetByEntityName_SuperAdmin("Alph");

            result.Should().HaveCount(1);
            result[0].EntityName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetByEntityName_SuperAdmin_Should_Handle_Null_SearchPattern()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ENT-00001", "Alpha");

            var result = await CreateQueryRepo().GetByEntityName_SuperAdmin(null!);

            result.Should().HaveCount(1);
        }
    }
}
