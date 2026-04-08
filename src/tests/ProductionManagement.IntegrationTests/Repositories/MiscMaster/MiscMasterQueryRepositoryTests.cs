using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedMiscTypeAsync(string code = "MT01", string desc = "Test Type")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            return miscType.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "M001", string desc = "Test Misc", int sortOrder = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = desc,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Production.ProductionPackDetail");
            await conn.ExecuteAsync("DELETE FROM Production.ProductionPackHeader");
            await conn.ExecuteAsync("DELETE FROM Production.LotMaster");
            await conn.ExecuteAsync("DELETE FROM Production.LotMaster");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(mtId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_MiscTypeId()
        {
            await ClearTablesAsync();
            var mtId1 = await SeedMiscTypeAsync("MT01", "Type One");
            var mtId2 = await SeedMiscTypeAsync("MT02", "Type Two");
            await SeedMiscMasterAsync(mtId1, "M001", "First");
            await SeedMiscMasterAsync(mtId2, "M002", "Second");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, mtId1);

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("M001");
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(mtId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(mtId, "M001", "Alpha Misc");
            await SeedMiscMasterAsync(mtId, "M002", "Beta Misc");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha", null);

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Alpha Misc");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_MiscTypeCode()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync("TCODE", "Type Desc");
            await SeedMiscMasterAsync(mtId, "M001", "Test");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null, null);

            items.Should().HaveCount(1);
            items[0].MiscTypeCode.Should().Be("TCODE");
            items[0].MiscTypeDescription.Should().Be("Type Desc");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync("MT01", "Test Type");
            var id = await SeedMiscMasterAsync(mtId, "M001", "ById Test", 3);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Code.Should().Be("M001");
            dto.Description.Should().Be("ById Test");
            dto.SortOrder.Should().Be(3);
            dto.MiscTypeCode.Should().Be("MT01");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(mtId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync("MT01", "Test Type");
            await SeedMiscMasterAsync(mtId, "M001", "Alpha Misc");
            await SeedMiscMasterAsync(mtId, "M002", "Beta Misc");

            var results = await CreateQueryRepo().AutocompleteAsync("Alpha", null, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Description.Should().Be("Alpha Misc");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_MiscTypeCode()
        {
            await ClearTablesAsync();
            var mtId1 = await SeedMiscTypeAsync("MT01", "Type One");
            var mtId2 = await SeedMiscTypeAsync("MT02", "Type Two");
            await SeedMiscMasterAsync(mtId1, "M001", "Item A");
            await SeedMiscMasterAsync(mtId2, "M002", "Item B");

            var results = await CreateQueryRepo().AutocompleteAsync("", "MT01", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Code.Should().Be("M001");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(mtId, "M001", "Inactive Test");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Inactive", null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(mtId, "DUPL01", "Existing");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPL01", mtId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Different_MiscType()
        {
            await ClearTablesAsync();
            var mtId1 = await SeedMiscTypeAsync("MT01", "Type One");
            var mtId2 = await SeedMiscTypeAsync("MT02", "Type Two");
            await SeedMiscMasterAsync(mtId1, "CODE01", "Existing");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("CODE01", mtId2);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(mtId, "SELF01", "Self");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("SELF01", mtId, id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(mtId, "DEL01", "Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DEL01", mtId);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(mtId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- MISC TYPE EXISTS ---

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(mtId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var exists = await CreateQueryRepo().MiscTypeExistsAsync(99999);

            exists.Should().BeFalse();
        }
    }
}
