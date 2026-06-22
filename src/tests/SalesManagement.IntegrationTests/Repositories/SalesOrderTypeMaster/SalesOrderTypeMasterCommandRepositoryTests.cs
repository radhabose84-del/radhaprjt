using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Constants;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrderTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrderTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class SalesOrderTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesOrderTypeMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesOrderTypeMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        // SoTypeId carries a real DB FK → Sales.MiscMaster (DeleteBehavior.Restrict).
        // Seed a MiscType (SOTM_TYPE) + MiscMaster row and reuse its Id as SoTypeId.
        private async Task<int> EnsureSoTypeIdAsync(string code = MiscMasterCodes.SO_NORMAL)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == MiscMasterCodes.SOTM_TYPE_MISCTYPE);
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = MiscMasterCodes.SOTM_TYPE_MISCTYPE,
                    Description = "Sales Order Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<SalesManagement.Domain.Entities.SalesOrderTypeMaster> BuildEntityAsync(
            string typeName = "Normal SO", int taxTypeId = 1)
        {
            var soTypeId = await EnsureSoTypeIdAsync();
            return new SalesManagement.Domain.Entities.SalesOrderTypeMaster
            {
                SoTypeId = soTypeId,
                TaxTypeId = taxTypeId,
                TypeName = typeName,
                Description = "desc",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Normal C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = await BuildEntityAsync("Normal C2", taxTypeId: 3);
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderTypeMaster.FirstAsync(x => x.Id == id);
            saved.TypeName.Should().Be("Normal C2");
            saved.TaxTypeId.Should().Be(3);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Normal C3"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderTypeMaster.FirstAsync(x => x.Id == id);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Normal U1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("Normal U1");
            entity.Id = id;
            entity.TypeName = "Updated";
            entity.Description = "New desc";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.SalesOrderTypeMaster.FirstAsync(x => x.Id == id);
            reloaded.TypeName.Should().Be("Updated");
            reloaded.Description.Should().Be("New desc");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_Immutable_Identifiers()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var original = await BuildEntityAsync("Normal IM", taxTypeId: 1);
            var id = await CreateRepo(ctx).CreateAsync(original);
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("Normal IM", taxTypeId: 1);
            entity.Id = id;
            entity.TaxTypeId = 999;     // attempt to change immutable identifier
            entity.TypeName = "x";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.SalesOrderTypeMaster.FirstAsync(x => x.Id == id);
            reloaded.SoTypeId.Should().Be(original.SoTypeId);
            reloaded.TaxTypeId.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = await BuildEntityAsync("Ghost");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Normal D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Normal D2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.SalesOrderTypeMaster.FirstAsync(x => x.Id == id);
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
