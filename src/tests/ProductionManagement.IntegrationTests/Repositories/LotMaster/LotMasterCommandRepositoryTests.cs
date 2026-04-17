using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.LotMaster;
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

        private async Task<(int LotTypeId, int StatusId)> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "LM_TYP");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "LM_TYP", Description = "LotType",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var lotType = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "LM_LT" && x.MiscTypeId == t.Id);
            if (lotType == null)
            {
                lotType = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "LM_LT", Description = "LT",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(lotType);
                await ctx.SaveChangesAsync();
            }
            var status = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "LM_ST" && x.MiscTypeId == t.Id);
            if (status == null)
            {
                status = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "LM_ST", Description = "ST",
                    SortOrder = 2, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(status);
                await ctx.SaveChangesAsync();
            }
            return (lotType.Id, status.Id);
        }

        private async Task<Domain.Entities.LotMaster> BuildEntityAsync(string code = "LM_C1")
        {
            var (lotTypeId, statusId) = await EnsureMiscAsync();
            return new Domain.Entities.LotMaster
            {
                LotCode = code,
                BatchNumber = code + "_B",
                LotTypeId = lotTypeId,
                StatusId = statusId,
                ItemId = 1,
                UnitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TotalProducedQty = 100m,
                AvailableQty = 100m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("LMC1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("LMC2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.LotMaster.FirstAsync(x => x.Id == id);

            saved.LotCode.Should().Be("LMC2");
            saved.BatchNumber.Should().Be("LMC2_B");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = await BuildEntityAsync("LMU1");
            entity.ProductionOrderRef = "PO_OLD";
            entity.Remarks = "Old";
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("LMU1");
            updated.Id = id;
            updated.ProductionOrderRef = "PO_NEW";
            updated.Remarks = "New";
            updated.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.LotMaster.FirstAsync(x => x.Id == id);
            reloaded.ProductionOrderRef.Should().Be("PO_NEW");
            reloaded.Remarks.Should().Be("New");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("LMGH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("LMD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_IsDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("LMD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.LotMaster.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
