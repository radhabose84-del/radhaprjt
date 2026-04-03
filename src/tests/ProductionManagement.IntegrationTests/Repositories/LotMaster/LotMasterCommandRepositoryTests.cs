using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.LotMaster;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.LotMaster
{
    [Collection("DatabaseCollection")]
    public sealed class LotMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LotMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private LotMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<(int lotTypeId, int statusId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "LOTMT",
                Description = "Lot Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var miscRepo = new MiscMasterCommandRepository(ctx);

            var lotTypeId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LTYPE",
                Description = "Lot Type",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var statusId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LSTAT",
                Description = "Lot Status",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            return (lotTypeId, statusId);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[LotMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private static Domain.Entities.LotMaster BuildEntity(
            int lotTypeId,
            int statusId,
            string lotCode = "LOT001",
            string batchNumber = "BATCH001",
            int itemId = 1,
            int unitId = 1) =>
            new()
            {
                LotCode = lotCode,
                BatchNumber = batchNumber,
                LotTypeId = lotTypeId,
                ItemId = itemId,
                UnitId = unitId,
                StartDate = new DateOnly(2026, 1, 15),
                StatusId = statusId,
                ProductionOrderRef = "PO-001",
                TotalProducedQty = 100.0m,
                AvailableQty = 80.0m,
                RunOutDate = new DateOnly(2026, 6, 30),
                Remarks = "Test lot",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(lotTypeId, statusId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(
                BuildEntity(lotTypeId, statusId, "LOT002", "BATCH002", 5, 3));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.LotMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.LotCode.Should().Be("LOT002");
            saved.BatchNumber.Should().Be("BATCH002");
            saved.LotTypeId.Should().Be(lotTypeId);
            saved.ItemId.Should().Be(5);
            saved.UnitId.Should().Be(3);
            saved.StartDate.Should().Be(new DateOnly(2026, 1, 15));
            saved.StatusId.Should().Be(statusId);
            saved.ProductionOrderRef.Should().Be("PO-001");
            saved.TotalProducedQty.Should().Be(100.0m);
            saved.AvailableQty.Should().Be(80.0m);
            saved.RunOutDate.Should().Be(new DateOnly(2026, 6, 30));
            saved.Remarks.Should().Be("Test lot");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(lotTypeId, statusId, "LOT003"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.LotMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Allow_Null_RunOutDate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(lotTypeId, statusId, "LOT004");
            entity.RunOutDate = null;
            var newId = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.LotMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.RunOutDate.Should().BeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotTypeId, statusId, "LOT005"));
            ctx.ChangeTracker.Clear();

            var entity = await ctx.LotMaster.FirstAsync(x => x.Id == id);
            entity.Remarks = "Updated Remarks";
            entity.ProductionOrderRef = "PO-UPDATED";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.LotMaster.FirstAsync(x => x.Id == id);
            updated.Remarks.Should().Be("Updated Remarks");
            updated.ProductionOrderRef.Should().Be("PO-UPDATED");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(1, 1, "GHOST");
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
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotTypeId, statusId, "LOT006"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotTypeId, statusId, "LOT007"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.LotMaster.IgnoreQueryFilters()
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
