using Dapper;
using Microsoft.Data.SqlClient;
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

        private TncTemplateMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.TnCTemplateMaster BuildEntity(
            string name = "Test Template",
            int templateTypeId = 1,
            string html = "<p>Terms</p>") =>
            new()
            {
                TemplateName = name,
                TemplateTypeId = templateTypeId,
                TermsHtml = html,
                ApprovalFlag = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.TnCTemplateApplicability");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.TnCTemplateMaster");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Name_And_Html()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("My Template", html: "<p>My Terms</p>"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TnCTemplateMaster.FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.TemplateName.Should().Be("My Template");
            saved.TermsHtml.Should().Be("<p>My Terms</p>");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TnCTemplateMaster.FirstOrDefaultAsync(x => x.Id == id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.TnCTemplateMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999);

            result.Should().BeFalse();
        }
    }
}
