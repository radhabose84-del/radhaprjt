using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.CommissionSplit;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.CommissionSplit
{
    [Collection("DatabaseCollection")]
    public sealed class CommissionSplitCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CommissionSplitCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CommissionSplitCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<(List<int> roleIds, int shareTypeId)> EnsureMiscAsync(int rolesNeeded = 5)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CSC_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CSC_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var roleIds = new List<int>();
            for (int i = 1; i <= rolesNeeded; i++)
            {
                var code = $"CSC_R{i}";
                var role = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
                if (role == null)
                {
                    role = new SalesManagement.Domain.Entities.MiscMaster
                    {
                        MiscTypeId = t.Id, Code = code, Description = "Role" + i,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    };
                    await ctx.MiscMaster.AddAsync(role);
                    await ctx.SaveChangesAsync();
                }
                roleIds.Add(role.Id);
            }
            var share = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CSC_SHARE");
            if (share == null)
            {
                share = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "CSC_SHARE", Description = "Percent",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(share);
                await ctx.SaveChangesAsync();
            }
            return (roleIds, share.Id);
        }

        private async Task<SalesManagement.Domain.Entities.CommissionSplit> BuildEntityAsync(
            string name = "Split", int detailCount = 2)
        {
            var (roleIds, shareTypeId) = await EnsureMiscAsync(Math.Max(detailCount, 5));
            var entity = new SalesManagement.Domain.Entities.CommissionSplit
            {
                SplitName = name,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                CommissionSplitDetails = Enumerable.Range(0, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.CommissionSplitDetail
                    {
                        RoleId = roleIds[i],
                        ShareTypeId = shareTypeId,
                        ShareValue = (i + 1) * 10m,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }).ToList()
            };
            return entity;
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CS1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Auto_Generate_SplitCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CS2"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CommissionSplit.FirstAsync(x => x.Id == id);
            saved.SplitCode.Should().NotBeNullOrEmpty();
            saved.SplitCode.Should().StartWith("CSP");
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Detail_Rows()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CS3", detailCount: 3));

            var details = await ctx.CommissionSplitDetail.Where(x => x.CommissionSplitId == id).ToListAsync();
            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("Original"));
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("Updated Name");
            updated.Id = id;
            updated.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.CommissionSplit.FirstAsync(x => x.Id == id);
            reloaded.SplitName.Should().Be("Updated Name");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CSR", detailCount: 2));
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("CSR", detailCount: 4);
            updated.Id = id;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            // Note: old details are removed, new ones added — but old remain with IsDeleted=0 if RemoveRange is physical
            // The repo uses RemoveRange which is physical delete for children
            var details = await ctx.CommissionSplitDetail
                .Where(x => x.CommissionSplitId == id && x.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();
            details.Should().HaveCount(4);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("Ghost");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Parent_And_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CSD"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var parent = await ctx.CommissionSplit.FirstAsync(x => x.Id == id);
            parent.IsDeleted.Should().Be(IsDelete.Deleted);
            var children = await ctx.CommissionSplitDetail
                .IgnoreQueryFilters()
                .Where(x => x.CommissionSplitId == id).ToListAsync();
            children.Should().AllSatisfy(c => c.IsDeleted.Should().Be(IsDelete.Deleted));
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
