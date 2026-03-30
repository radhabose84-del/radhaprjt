using Dapper;
using Microsoft.Data.SqlClient;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.UsageType;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace InventoryManagement.IntegrationTests.Repositories.UsageType
{
    [Collection("DatabaseCollection")]
    public sealed class UsageTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UsageTypeQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UsageTypeQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var moduleLookupMock = new Mock<IModuleLookup>(MockBehavior.Loose);
            moduleLookupMock.Setup(m => m.GetAllModuleAsync())
                .ReturnsAsync(new List<ModuleLookupDto>
                {
                    new ModuleLookupDto { ModuleId = 1, ModuleName = "Test Module" }
                });
            return new UsageTypeQueryRepository(conn, moduleLookupMock.Object);
        }

        private async Task<int> SeedEntityAsync(string code = "USAGE_QRY001", string name = "Query Test Usage", int moduleId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new UsageTypeCommandRepository(ctx);
            return await repo.CreateAsync(new InventoryManagement.Domain.Entities.UsageType
            {
                UsageTypeCode = code,
                UsageTypeName = name,
                Description = "Test",
                ModuleId = moduleId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UsageType]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("USAGE_DEL1", "Delete Me");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UsageTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("USAGE_A1", "Alpha Usage");
            await SeedEntityAsync("USAGE_B1", "Beta Usage");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "USAGE_A1");

            items.Should().HaveCount(1);
            items[0].UsageTypeCode.Should().Be("USAGE_A1");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("USAGE_ID1", "Get By Id Usage");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.UsageTypeCode.Should().Be("USAGE_ID1");
            result.UsageTypeName.Should().Be("Get By Id Usage");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("USAGE_DEL2", "Soft Deleted Usage");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UsageTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("USAGE_EX1", "Existing Usage");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("USAGE_EX1");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("USAGE_EX2", "To Delete Usage");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UsageTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("USAGE_EX2");

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("USAGE_AC1", "Autocomplete Usage");

            var results = await CreateQueryRepo().AutocompleteAsync("Autocomplete", CancellationToken.None);

            results.Should().NotBeEmpty();
            results[0].UsageTypeName.Should().Be("Autocomplete Usage");
        }
    }
}
