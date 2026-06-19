using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.CostCentre;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.CostCentre
{
    [Collection("DatabaseCollection")]
    public sealed class CostCentreCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CostCentreCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static CostCentreCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.CostCentre BuildEntity(
            int centreLevelId,
            string code = "STP",
            string name = "Plant",
            int unitId = 1,
            int? parentId = null) =>
            new()
            {
                UnitId = unitId,
                CompanyId = 1,
                CostCentreCode = code,
                CostCentreName = name,
                CentreLevelId = centreLevelId,
                ParentCostCentreId = parentId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedLevelAsync(int sortOrder = 1, string code = "CCL1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "COSTCENTRELEVEL");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "COSTCENTRELEVEL",
                    Description = "Cost Centre Level",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(type);
                await ctx.SaveChangesAsync();
            }

            var misc = new Domain.Entities.MiscMaster
            {
                MiscTypeId = type.Id,
                Code = code,
                Description = code,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId, "STP", "Plant", 1));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CostCentre.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CostCentreCode.Should().Be("STP");
            saved.CostCentreName.Should().Be("Plant");
            saved.UnitId.Should().Be(1);
            saved.CentreLevelId.Should().Be(levelId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CostCentre.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_NameAndStatus()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(levelId);
            entity.Id = id;
            entity.CostCentreName = "Plant Edited";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.CostCentre.FirstAsync(x => x.Id == id);
            updated.CostCentreName.Should().Be("Plant Edited");
            updated.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId, "STP", "Plant"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(levelId, "HACKED", "Plant Edited");
            entity.Id = id;

            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.CostCentre.FirstAsync(x => x.Id == id);
            updated.CostCentreCode.Should().Be("STP");          // immutable
            updated.CostCentreName.Should().Be("Plant Edited");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(levelId);
            entity.Id = 9999;

            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.CostCentre.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
