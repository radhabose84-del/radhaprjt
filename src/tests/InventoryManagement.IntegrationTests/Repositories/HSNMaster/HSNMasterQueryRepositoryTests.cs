using Dapper;
using Microsoft.Data.SqlClient;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.HSNMaster;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace InventoryManagement.IntegrationTests.Repositories.HSNMaster
{
    [Collection("DatabaseCollection")]
    public sealed class HSNMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public HSNMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private HSNMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new HSNMasterQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "Test Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int typeId, int gstCategoryId, string hsnCode = "1001", string description = "Test HSN")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new HSNMasterCommandRepository(ctx);
            return await repo.CreateAsync(new InventoryManagement.Domain.Entities.HSNMaster
            {
                TypeId = typeId,
                GSTCategoryId = gstCategoryId,
                HSNCode = hsnCode,
                Description = description,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[HSNMaster]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[IssueDetail]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[IssueHeader]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MrsDetail]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MrsHeader]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOMConversion]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOM]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q1");
            await SeedEntityAsync(miscId, miscId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q2");
            var id = await SeedEntityAsync(miscId, miscId, "2001", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new HSNMasterCommandRepository(ctx).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.HSNMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q3");
            await SeedEntityAsync(miscId, miscId, "3001", "Alpha HSN");
            await SeedEntityAsync(miscId, miscId, "3002", "Beta HSN");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].HSNCode.Should().Be("3001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_PaginationMetadata()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T4");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q4");
            await SeedEntityAsync(miscId, miscId, "4001", "Page HSN 1");
            await SeedEntityAsync(miscId, miscId, "4002", "Page HSN 2");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 1, null);

            items.Should().HaveCount(1);
            total.Should().Be(2);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T5");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q5");
            var id = await SeedEntityAsync(miscId, miscId, "5001", "Get By Id HSN");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.HSNCode.Should().Be("5001");
            result.Description.Should().Be("Get By Id HSN");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T6");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q6");
            var id = await SeedEntityAsync(miscId, miscId, "6001", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new HSNMasterCommandRepository(ctx).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.HSNMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T7");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q7");
            await SeedEntityAsync(miscId, miscId, "7001", "Existing");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("7001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T8");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q8");
            var id = await SeedEntityAsync(miscId, miscId, "8001", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new HSNMasterCommandRepository(ctx).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.HSNMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var exists = await CreateQueryRepo().AlreadyExistsAsync("8001");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_ExcludingSelf()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T9");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q9");
            var id = await SeedEntityAsync(miscId, miscId, "9001", "Self Check");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("9001", id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Record_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_QRY_T10");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "HSN_Q10");
            var id = await SeedEntityAsync(miscId, miscId, "1010");

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Record_Missing()
        {
            await ClearTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }
    }
}
