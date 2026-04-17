using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Repositories.UOMs;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.UOMsTests
{
    [Collection("DatabaseCollection")]
    public sealed class UOMCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> EnsureUomTypeMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "UOM_CMD_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UOM_CMD_T", Description = "UOM Cmd Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "UCMDT");
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = "UCMDT", Description = "UOMCmd",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private async Task<UOM> BuildEntityAsync(string code = "UCMD", string name = "NameCmd")
        {
            var typeId = await EnsureUomTypeMiscAsync();
            return new UOM
            {
                Code = code,
                UOMName = name,
                UOMTypeId = typeId,
                SortOrder = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("C1", "Name1"));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Auto_Set_SortOrder()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var r1 = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SO1", "Name SO1"));
            var r2 = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SO2", "Name SO2"));

            r2.SortOrder.Should().Be(r1.SortOrder + 1);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("AUD1", "AuditName"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.UOMs.FirstAsync(x => x.Id == result.Id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("UPD1", "Before");
            var created = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            created.UOMName = "After";
            var result = await CreateRepo(ctx).UpdateAsync(created);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.UOMs.FirstAsync(x => x.Id == created.Id);
            reloaded.UOMName.Should().Be("After");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var ghost = await BuildEntityAsync("GH", "Ghost");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().BeFalse();
        }

        // --- DeleteAsync (SoftDelete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DEL1", "DelName");
            var created = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            created.IsDeleted = IsDelete.Deleted;
            var result = await CreateRepo(ctx).DeleteAsync(created.Id, created);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Flag_Record()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DEL2", "DelName2");
            var created = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            created.IsDeleted = IsDelete.Deleted;
            await CreateRepo(ctx).DeleteAsync(created.Id, created);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.UOMs.FirstAsync(x => x.Id == created.Id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("GHD", "GhostDel");

            var result = await CreateRepo(ctx).DeleteAsync(9999999, entity);

            result.Should().BeFalse();
        }

        // --- CheckForDuplicatesAsync ---

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Return_True_When_Name_Duplicate()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DUP1", "DupName");
            await CreateRepo(ctx).CreateAsync(entity);

            var result = await CreateRepo(ctx).CheckForDuplicatesAsync("DupName", 0, 0);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DUP2", "SelfName");
            var created = await CreateRepo(ctx).CreateAsync(entity);

            var result = await CreateRepo(ctx).CheckForDuplicatesAsync("SelfName", 0, created.Id);

            result.Should().BeFalse();
        }

        // --- GetMaxSortOrderAsync ---

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_When_Empty()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetMaxSortOrderAsync();

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("M1", "M1"));
            await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("M2", "M2"));

            var result = await CreateRepo(ctx).GetMaxSortOrderAsync();

            result.Should().BeGreaterOrEqualTo(2);
        }
    }
}
