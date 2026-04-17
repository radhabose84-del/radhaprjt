using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.StoTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class StoTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public StoTypeMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StoTypeMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        // MovementTypeConfig requires MiscMaster rows for MovementCategoryId, FromStockTypeId, ToStockTypeId.
        // We seed a single MiscType + MiscMaster and reuse for all required FKs.
        private async Task<int> EnsureMiscIdAsync(string code = "STO_MISC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "STO_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "STO_MT", Description = "Sto Misc",
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

        private async Task<(int Pgi, int Gr)> EnsureMovementTypesAsync()
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var pgi = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "PGI");
            if (pgi == null)
            {
                pgi = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "PGI", MovementDescription = "PGI Move",
                    MovementCategoryId = miscId,
                    FromStockTypeId = miscId,
                    ToStockTypeId = miscId,
                    QuantityUpdateFlag = true,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(pgi);
                await ctx.SaveChangesAsync();
            }
            var gr = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "GR_S");
            if (gr == null)
            {
                gr = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "GR_S", MovementDescription = "GR Move",
                    MovementCategoryId = miscId,
                    FromStockTypeId = miscId,
                    ToStockTypeId = miscId,
                    QuantityUpdateFlag = true,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(gr);
                await ctx.SaveChangesAsync();
            }
            return (pgi.Id, gr.Id);
        }

        private async Task<SalesManagement.Domain.Entities.StoTypeMaster> BuildEntityAsync(string code = "STM1")
        {
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            return new SalesManagement.Domain.Entities.StoTypeMaster
            {
                StoTypeCode = code,
                StoTypeName = $"Type {code}",
                Description = "desc",
                PgiMovementTypeId = pgiId,
                GrMovementTypeId = grId,
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

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STM_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STM_C2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.StoTypeMaster.FirstAsync(x => x.Id == id);

            saved.StoTypeCode.Should().Be("STM_C2");
            saved.StoTypeName.Should().Be("Type STM_C2");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STM_U1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("STM_U1");
            entity.Id = id;
            entity.StoTypeName = "Updated";
            entity.Description = "New desc";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.StoTypeMaster.FirstAsync(x => x.Id == id);
            reloaded.StoTypeName.Should().Be("Updated");
            reloaded.Description.Should().Be("New desc");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STM_IM"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("STM_IM");
            entity.Id = id;
            entity.StoTypeCode = "CHANGED";
            entity.StoTypeName = "x";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.StoTypeMaster.FirstAsync(x => x.Id == id);
            reloaded.StoTypeCode.Should().Be("STM_IM");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = await BuildEntityAsync("GH");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STM_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STM_D2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.StoTypeMaster.FirstAsync(x => x.Id == id);
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
