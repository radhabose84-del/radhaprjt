using Contracts.Dtos.Lookups.Production;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.PackType
{
    [Collection("DatabaseCollection")]
    public sealed class PackTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PackTypeQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PackTypeQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedPackMaterialAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "PKMT",
                Description = "Pack Material Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            return await new MiscMasterCommandRepository(ctx).CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "COTTON",
                Description = "Cotton Material",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedPackTypeAsync(
            string code = "PT001",
            string name = "Test PackType",
            int? packMaterialId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new PackTypeCommandRepository(ctx).CreateAsync(new Domain.Entities.PackType
            {
                PackTypeCode = code,
                PackTypeName = name,
                NetWeight = 10.0m,
                TareWeight = 1.0m,
                GrossWeight = 11.0m,
                ConesPerBag = 24,
                PackMaterialId = packMaterialId,
                ProductionAllowed = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Production].[PackType]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedPackTypeAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_PackMaterialName()
        {
            await ClearTablesAsync();
            var packMatId = await SeedPackMaterialAsync();
            await SeedPackTypeAsync("PT001", "With Material", packMatId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].PackMaterialName.Should().Be("Cotton Material");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedPackTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new PackTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedPackTypeAsync("PT001", "Alpha Pack");
            await SeedPackTypeAsync("PT002", "Beta Pack");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].PackTypeName.Should().Be("Alpha Pack");
        }

        [Fact]
        public async Task GetAllAsync_Should_Handle_Null_PackMaterialId()
        {
            await ClearTablesAsync();
            await SeedPackTypeAsync("PT001", "No Material", null);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].PackMaterialId.Should().BeNull();
            items[0].PackMaterialName.Should().BeNull();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var id = await SeedPackTypeAsync("PT001", "ById Test");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.PackTypeCode.Should().Be("PT001");
            dto.PackTypeName.Should().Be("ById Test");
            dto.NetWeight.Should().Be(10.0m);
            dto.ConesPerBag.Should().Be(24);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedPackTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new PackTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

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
            await SeedPackTypeAsync("PT001", "Alpha Pack");
            await SeedPackTypeAsync("PT002", "Beta Pack");

            var results = await CreateQueryRepo().AutocompleteAsync("Alpha", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].PackTypeName.Should().Be("Alpha Pack");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var id = await SeedPackTypeAsync("PT001", "Inactive Pack");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.PackType.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Inactive", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            await SeedPackTypeAsync("DUPL01", "Existing");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPL01");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearTablesAsync();
            var id = await SeedPackTypeAsync("SELF01", "Self");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("SELF01", id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedPackTypeAsync("DEL01", "Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new PackTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DEL01");

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
            var id = await SeedPackTypeAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- PACK MATERIAL EXISTS ---

        [Fact]
        public async Task PackMaterialExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var packMatId = await SeedPackMaterialAsync();

            var exists = await CreateQueryRepo().PackMaterialExistsAsync(packMatId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task PackMaterialExistsAsync_Should_Return_False_When_Missing()
        {
            var exists = await CreateQueryRepo().PackMaterialExistsAsync(99999);

            exists.Should().BeFalse();
        }
    }
}
