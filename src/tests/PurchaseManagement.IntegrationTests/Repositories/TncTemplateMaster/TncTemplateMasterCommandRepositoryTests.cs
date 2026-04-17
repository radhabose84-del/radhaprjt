using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.TncTemplateMaster
{
    [Collection("DatabaseCollection")]
    public sealed class TncTemplateMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TncTemplateMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private TncTemplateMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMiscTypeIdAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == code);
            if (existing != null) return existing.Id;
            var t = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = code,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task<int> EnsureMiscIdAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == code && m.MiscTypeId == miscTypeId);
            if (existing != null) return existing.Id;
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = code,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<TnCTemplateMaster> BuildEntityAsync(string templateName)
        {
            var typeId = await EnsureMiscTypeIdAsync("TNC_TT");
            var typeRefId = await EnsureMiscIdAsync(typeId, "PURCHASE_TT");
            return new TnCTemplateMaster
            {
                TemplateCode = $"TC_{Guid.NewGuid():N}".Substring(0, 12),
                TemplateName = templateName,
                TemplateTypeId = typeRefId,
                TermsHtml = "<p>Sample terms</p>",
                ApprovalFlag = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task<int> EnsureApplicabilityMiscAsync(string code)
        {
            var typeId = await EnsureMiscTypeIdAsync("TNC_APP_T");
            return await EnsureMiscIdAsync(typeId, code);
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("TC_C1"), CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Master_And_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = await BuildEntityAsync("TC_C2");
            var appId = await EnsureApplicabilityMiscAsync("APP_C2");
            entity.Applicabilities = new List<TnCTemplateApplicability>
            {
                new() { ApplicabilityId = appId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TnCTemplateMaster.Include(t => t.Applicabilities).FirstAsync(x => x.Id == id);
            saved.TemplateName.Should().Be("TC_C2");
            saved.Applicabilities.Should().ContainSingle();
            saved.Applicabilities!.First().ApplicabilityId.Should().Be(appId);
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("GH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost, new List<TnCTemplateApplicability>());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Scalar_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("TC_U1"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var incoming = await BuildEntityAsync("TC_U1_New");
            incoming.Id = id;
            incoming.TermsHtml = "<p>Updated</p>";
            incoming.ApprovalFlag = true;

            var result = await CreateRepo(ctx).UpdateAsync(incoming, new List<TnCTemplateApplicability>());
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.TnCTemplateMaster.FirstAsync(x => x.Id == id);
            reloaded.TemplateName.Should().Be("TC_U1_New");
            reloaded.TermsHtml.Should().Be("<p>Updated</p>");
            reloaded.ApprovalFlag.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = await BuildEntityAsync("TC_U2");
            var oldAppId = await EnsureApplicabilityMiscAsync("APP_OLD");
            entity.Applicabilities = new List<TnCTemplateApplicability>
            {
                new() { ApplicabilityId = oldAppId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var newAppId = await EnsureApplicabilityMiscAsync("APP_NEW");
            var incoming = await BuildEntityAsync("TC_U2");
            incoming.Id = id;
            var newApps = new List<TnCTemplateApplicability>
            {
                new() { ApplicabilityId = newAppId }
            };
            await CreateRepo(ctx).UpdateAsync(incoming, newApps);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TnCTemplateMaster.Include(t => t.Applicabilities).FirstAsync(x => x.Id == id);
            saved.Applicabilities!.Should().ContainSingle();
            saved.Applicabilities!.First().ApplicabilityId.Should().Be(newAppId);
        }

        // --- SoftDeleteAsync ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("TC_D1"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Mark_Master_And_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = await BuildEntityAsync("TC_D2");
            var appId = await EnsureApplicabilityMiscAsync("APP_DEL");
            entity.Applicabilities = new List<TnCTemplateApplicability>
            {
                new() { ApplicabilityId = appId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.TnCTemplateMaster.Include(t => t.Applicabilities).FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
            reloaded.IsActive.Should().Be(Status.Inactive);
            reloaded.Applicabilities!.Should().AllSatisfy(a =>
            {
                a.IsDeleted.Should().Be(IsDelete.Deleted);
                a.IsActive.Should().Be(Status.Inactive);
            });
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999);

            result.Should().BeFalse();
        }
    }
}
