using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.CountMaster;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.CountMaster
{
    [Collection("DatabaseCollection")]
    public sealed class CountMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CountMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CountMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeAsync(ApplicationDbContext ctx, string code = "MT01")
        {
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return miscType.Id;
        }

        private async Task<int> SeedMiscMasterAsync(ApplicationDbContext ctx, int miscTypeId, string code = "MM01")
        {
            var repo = new MiscMasterCommandRepository(ctx);
            var id = await repo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "Test Misc",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();
            return id;
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[CountMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private static Domain.Entities.CountMaster BuildEntity(
            int countTypeId,
            string code = "C001",
            decimal countValue = 10.5m,
            string shortName = "TST",
            int? countCategoryId = null,
            string desc = "Test Count",
            int uomId = 1) =>
            new()
            {
                CountCode = code,
                CountValue = countValue,
                ShortName = shortName,
                CountCategoryId = countCategoryId,
                CountTypeId = countTypeId,
                CountDescription = desc,
                UOMId = uomId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var miscId = await SeedMiscMasterAsync(ctx, mtId);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var countTypeId = await SeedMiscMasterAsync(ctx, mtId, "CTYPE");
            var catId = await SeedMiscMasterAsync(ctx, mtId, "CCAT");

            var newId = await CreateRepo(ctx).CreateAsync(
                BuildEntity(countTypeId, "C002", 25.5m, "SHT", catId, "Detailed Count", 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CountMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CountCode.Should().Be("C002");
            saved.CountValue.Should().Be(25.5m);
            saved.ShortName.Should().Be("SHT");
            saved.CountCategoryId.Should().Be(catId);
            saved.CountTypeId.Should().Be(countTypeId);
            saved.CountDescription.Should().Be("Detailed Count");
            saved.UOMId.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var miscId = await SeedMiscMasterAsync(ctx, mtId);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "C003"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CountMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Allow_Null_CountCategoryId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var miscId = await SeedMiscMasterAsync(ctx, mtId);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "C004", countCategoryId: null));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CountMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CountCategoryId.Should().BeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var miscId = await SeedMiscMasterAsync(ctx, mtId);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "C005", desc: "Original"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.CountMaster.FirstAsync(x => x.Id == id);
            entity.CountDescription = "Updated";
            entity.CountValue = 99.9m;
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.CountMaster.FirstAsync(x => x.Id == id);
            updated.CountDescription.Should().Be("Updated");
            updated.CountValue.Should().Be(99.9m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(1, "GHOST");
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
            var mtId = await SeedMiscTypeAsync(ctx);
            var miscId = await SeedMiscMasterAsync(ctx, mtId);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "C006"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mtId = await SeedMiscTypeAsync(ctx);
            var miscId = await SeedMiscMasterAsync(ctx, mtId);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "C007"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.CountMaster.IgnoreQueryFilters()
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
