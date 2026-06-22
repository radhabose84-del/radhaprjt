using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.ProfitCentre;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.ProfitCentre
{
    [Collection("DatabaseCollection")]
    public sealed class ProfitCentreCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProfitCentreCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static ProfitCentreCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.ProfitCentre BuildEntity(
            int levelId,
            string code = "PCSPIN",
            string name = "Spinning",
            int? parentId = null) =>
            new()
            {
                CompanyId = 1,
                ProfitCentreCode = code,
                ProfitCentreName = name,
                LevelId = levelId,
                ParentProfitCentreId = parentId,
                IsRevenueLinked = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedLevelAsync(int sortOrder = 1, string code = "PCL1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "PROFITCENTRELEVEL");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PROFITCENTRELEVEL",
                    Description = "Profit Centre Level",
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId, "PCSPIN", "Spinning"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProfitCentre.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ProfitCentreCode.Should().Be("PCSPIN");
            saved.ProfitCentreName.Should().Be("Spinning");
            saved.CompanyId.Should().Be(1);
            saved.LevelId.Should().Be(levelId);
            saved.IsRevenueLinked.Should().BeTrue();
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

            var saved = await ctx.ProfitCentre.FirstOrDefaultAsync(x => x.Id == newId);

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
            entity.ProfitCentreName = "Spinning Edited";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.ProfitCentre.FirstAsync(x => x.Id == id);
            updated.ProfitCentreName.Should().Be("Spinning Edited");
            updated.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(levelId, "PCSPIN", "Spinning"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(levelId, "HACKED", "Spinning Edited");
            entity.Id = id;

            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ProfitCentre.FirstAsync(x => x.Id == id);
            updated.ProfitCentreCode.Should().Be("PCSPIN");          // immutable
            updated.ProfitCentreName.Should().Be("Spinning Edited");
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

            var deleted = await ctx.ProfitCentre.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);

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
