using Microsoft.EntityFrameworkCore;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DiscountMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DiscountMaster
{
    [Collection("DatabaseCollection")]
    public sealed class DiscountMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DiscountMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DiscountMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMiscIdAsync(string code = "DM_MISC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DM_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DM_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<SalesManagement.Domain.Entities.DiscountMaster> BuildEntityAsync(string name = "Disc1")
        {
            var miscId = await EnsureMiscIdAsync();
            return new SalesManagement.Domain.Entities.DiscountMaster
            {
                DiscountName = name,
                TriggerEventId = miscId,
                DiscountBasisId = miscId,
                ExecutionTypeId = miscId,
                Priority = 1,
                ValueTypeId = miscId,
                SlabTypeId = miscId,
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

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_DiscountCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_C2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.DiscountMaster.FirstAsync(x => x.Id == id);

            saved.DiscountCode.Should().StartWith("DC");
            saved.DiscountCode!.Length.Should().Be(7); // DC + 5 digits
        }

        [Fact]
        public async Task CreateAsync_Should_Increment_DiscountCode_Sequence()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id1 = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_S1"));
            var id2 = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_S2"));
            ctx.ChangeTracker.Clear();

            var d1 = await ctx.DiscountMaster.FirstAsync(x => x.Id == id1);
            var d2 = await ctx.DiscountMaster.FirstAsync(x => x.Id == id2);

            // Codes should differ
            d1.DiscountCode.Should().NotBe(d2.DiscountCode);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_U1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("DC_Updated");
            entity.Id = id;
            entity.Priority = 5;
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.DiscountMaster.FirstAsync(x => x.Id == id);
            reloaded.DiscountName.Should().Be("DC_Updated");
            reloaded.Priority.Should().Be(5);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_DiscountCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_IM"));
            ctx.ChangeTracker.Clear();
            var origCode = (await ctx.DiscountMaster.FirstAsync(x => x.Id == id)).DiscountCode;

            var entity = await BuildEntityAsync("DC_IM_Updated");
            entity.Id = id;
            entity.DiscountCode = "OVERWRITTEN";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.DiscountMaster.FirstAsync(x => x.Id == id);
            reloaded.DiscountCode.Should().Be(origCode);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = await BuildEntityAsync("DC_CH");
            entity.DiscountSlabs = new List<DiscountSlab>
            {
                new() { FromValue = 0m, ToValue = 100m, DiscountValue = 5m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var update = await BuildEntityAsync("DC_CH");
            update.Id = id;
            update.DiscountSlabs = new List<DiscountSlab>
            {
                new() { SlabOrder = 1, FromValue = 100m, ToValue = 500m, DiscountValue = 10m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                new() { SlabOrder = 2, FromValue = 500m, ToValue = 1000m, DiscountValue = 15m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            await CreateRepo(ctx).UpdateAsync(update);
            ctx.ChangeTracker.Clear();

            var slabs = await ctx.DiscountSlab.Where(s => s.DiscountMasterId == id).ToListAsync();
            slabs.Should().HaveCount(2);
            slabs.Select(s => s.FromValue).Should().BeEquivalentTo(new[] { 100m, 500m });
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("GH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("DC_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Cascade_To_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = await BuildEntityAsync("DC_DCH");
            entity.DiscountSlabs = new List<DiscountSlab>
            {
                new() { FromValue = 0m, ToValue = 100m, DiscountValue = 5m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.DiscountMaster.FirstAsync(x => x.Id == id);
            var slabs = await ctx.DiscountSlab.Where(s => s.DiscountMasterId == id).ToListAsync();
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
            slabs.Should().AllSatisfy(s => s.IsDeleted.Should().Be(IsDelete.Deleted));
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
