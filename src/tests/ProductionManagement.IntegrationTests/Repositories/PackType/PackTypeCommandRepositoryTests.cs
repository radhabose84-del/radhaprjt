using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.PackType
{
    [Collection("DatabaseCollection")]
    public sealed class PackTypeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PackTypeCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PackTypeCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedPackMaterialAsync(ApplicationDbContext ctx)
        {
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

            var miscMaster = new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "COTTON",
                Description = "Cotton Material",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var id = await new MiscMasterCommandRepository(ctx).CreateAsync(miscMaster);
            ctx.ChangeTracker.Clear();
            return id;
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[PackType]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private static Domain.Entities.PackType BuildEntity(
            string code = "PT001",
            string name = "Test PackType",
            decimal netWeight = 10.0m,
            decimal tareWeight = 1.0m,
            decimal grossWeight = 11.0m,
            int? conesPerBag = 24,
            int? packMaterialId = null,
            bool productionAllowed = true) =>
            new()
            {
                PackTypeCode = code,
                PackTypeName = name,
                NetWeight = netWeight,
                TareWeight = tareWeight,
                GrossWeight = grossWeight,
                ConesPerBag = conesPerBag,
                PackMaterialId = packMaterialId,
                ProductionAllowed = productionAllowed,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var packMatId = await SeedPackMaterialAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(
                BuildEntity("PT002", "Bag Type A", 20.0m, 2.0m, 22.0m, 48, packMatId, false));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PackType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PackTypeCode.Should().Be("PT002");
            saved.PackTypeName.Should().Be("Bag Type A");
            saved.NetWeight.Should().Be(20.0m);
            saved.TareWeight.Should().Be(2.0m);
            saved.GrossWeight.Should().Be(22.0m);
            saved.ConesPerBag.Should().Be(48);
            saved.PackMaterialId.Should().Be(packMatId);
            saved.ProductionAllowed.Should().BeFalse();
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("PT003"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PackType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Allow_Null_PackMaterialId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("PT004", packMaterialId: null));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PackType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PackMaterialId.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Allow_Null_ConesPerBag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("PT005", conesPerBag: null));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PackType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ConesPerBag.Should().BeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PT006", "Original"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.PackType.FirstAsync(x => x.Id == id);
            entity.PackTypeName = "Updated Name";
            entity.NetWeight = 50.0m;
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.PackType.FirstAsync(x => x.Id == id);
            updated.PackTypeName.Should().Be("Updated Name");
            updated.NetWeight.Should().Be(50.0m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity("GHOST");
            entity.Id = 99999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PT007"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PT008"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.PackType.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
