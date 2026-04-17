using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Repositories.HSNMaster;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.HSNMasterTests
{
    [Collection("DatabaseCollection")]
    public sealed class HSNMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public HSNMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private HSNMasterCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> EnsureMiscMasterAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "HSN_CMD_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "HSN_CMD_T", Description = "HSN Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "HSNCMD");
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = "HSNCMD", Description = "HSN Cmd",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private async Task<HSNMaster> BuildEntityAsync(string code = "CMD1")
        {
            var miscId = await EnsureMiscMasterAsync();
            return new HSNMaster
            {
                HSNCode = code,
                Description = $"HSN {code}",
                TypeId = miscId,
                GSTCategoryId = miscId,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("HCMD1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("HCMD2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.HSNMaster.FirstAsync(x => x.Id == id);

            saved.HSNCode.Should().Be("HCMD2");
            saved.GSTPercentage.Should().Be(18m);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoPopulate_AuditFields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("HCMD3"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.HSNMaster.FirstAsync(x => x.Id == id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("HCMDU1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("HCMDU1_NEW");
            entity.Id = id;
            entity.Description = "Updated desc";
            entity.GSTPercentage = 12m;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.HSNMaster.FirstAsync(x => x.Id == id);
            reloaded.Description.Should().Be("Updated desc");
            reloaded.GSTPercentage.Should().Be(12m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var ghost = await BuildEntityAsync("GHOST");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_SoftDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("SD_UPD");
            var id = await CreateRepo(ctx).CreateAsync(entity);
            await CreateRepo(ctx).DeleteAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var upd = await BuildEntityAsync("SD_UPD_NEW");
            upd.Id = id;
            var result = await CreateRepo(ctx).UpdateAsync(upd);

            result.Should().Be(0);
        }

        // --- DeleteAsync (SoftDelete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DEL1");
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).DeleteAsync(id, entity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Flag_IsDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DEL2");
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).DeleteAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.HSNMaster.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("DELGHOST");

            var result = await CreateRepo(ctx).DeleteAsync(9999999, entity);

            result.Should().BeFalse();
        }
    }
}
