using Microsoft.EntityFrameworkCore;
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

        // ModuleId / TransactionTypeId are cross-module FKs with NO DB constraint,
        // so plain integer values are valid for these tests.
        private const int ModuleId = 1;

        private static TnCTemplateMaster BuildEntity(string templateName, int moduleId = ModuleId) =>
            new TnCTemplateMaster
            {
                TemplateCode = $"TC_{Guid.NewGuid():N}".Substring(0, 12),
                TemplateName = templateName,
                ModuleId = moduleId,
                TermsHtml = "<p>Sample terms</p>",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TC_C1"), CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Master_And_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = BuildEntity("TC_C2");
            const int txnTypeId = 101;
            entity.Applicabilities = new List<TnCTemplateApplicability>
            {
                new() { TransactionTypeId = txnTypeId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TnCTemplateMaster.Include(t => t.Applicabilities).FirstAsync(x => x.Id == id);
            saved.TemplateName.Should().Be("TC_C2");
            saved.ModuleId.Should().Be(ModuleId);
            saved.Applicabilities.Should().ContainSingle();
            saved.Applicabilities!.First().TransactionTypeId.Should().Be(txnTypeId);
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = BuildEntity("GH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost, new List<TnCTemplateApplicability>());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Scalar_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TC_U1"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var incoming = BuildEntity("TC_U1_New", moduleId: 2);
            incoming.Id = id;
            incoming.TermsHtml = "<p>Updated</p>";

            var result = await CreateRepo(ctx).UpdateAsync(incoming, new List<TnCTemplateApplicability>());
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.TnCTemplateMaster.FirstAsync(x => x.Id == id);
            reloaded.TemplateName.Should().Be("TC_U1_New");
            reloaded.ModuleId.Should().Be(2);
            reloaded.TermsHtml.Should().Be("<p>Updated</p>");
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = BuildEntity("TC_U2");
            const int oldTxnId = 201;
            entity.Applicabilities = new List<TnCTemplateApplicability>
            {
                new() { TransactionTypeId = oldTxnId, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var id = await CreateRepo(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            const int newTxnId = 202;
            var incoming = BuildEntity("TC_U2");
            incoming.Id = id;
            var newApps = new List<TnCTemplateApplicability>
            {
                new() { TransactionTypeId = newTxnId }
            };
            await CreateRepo(ctx).UpdateAsync(incoming, newApps);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TnCTemplateMaster.Include(t => t.Applicabilities).FirstAsync(x => x.Id == id);
            saved.Applicabilities!.Should().ContainSingle();
            saved.Applicabilities!.First().TransactionTypeId.Should().Be(newTxnId);
        }

        // --- SoftDeleteAsync ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TC_D1"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Mark_Master_And_Children()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var entity = BuildEntity("TC_D2");
            entity.Applicabilities = new List<TnCTemplateApplicability>
            {
                new() { TransactionTypeId = 301, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
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
