using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Data.Repositories.Item.Templates;

namespace InventoryManagement.IntegrationTests.Repositories.Templates
{
    [Collection("DatabaseCollection")]
    public sealed class TemplateQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TemplateQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private TemplateQueryRepository CreateQueryRepo(ApplicationDbContext ctx) => new(ctx);
        private TemplateCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedTemplateAsync(string name = "Template001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(
                new InspectionTemplate
                {
                    TemplateName = name,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                }, CancellationToken.None);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx) => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByIdAsync_Should_Return_Template_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await SeedTemplateAsync("Find Me");

            var result = await CreateQueryRepo(ctx).GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.TemplateName.Should().Be("Find Me");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateQueryRepo(ctx).GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedTemplateAsync("Template A");

            var (items, total) = await CreateQueryRepo(ctx).GetAllAsync(null, 1, 10, CancellationToken.None);

            items.Should().HaveCountGreaterThanOrEqualTo(1);
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await SeedTemplateAsync("Deleted Template");

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.InspectionTemplate.FirstAsync(x => x.Id == id);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx2.SaveChangesAsync();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var (items, total) = await CreateQueryRepo(ctx3).GetAllAsync(null, 1, 10, CancellationToken.None);

            items.Should().NotContain(t => t.Id == id);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedTemplateAsync("Alpha Template");
            await SeedTemplateAsync("Beta Template");

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var (items, total) = await CreateQueryRepo(ctx2).GetAllAsync("Alpha", 1, 10, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].TemplateName.Should().Be("Alpha Template");
        }

        [Fact]
        public async Task GetAutoCompleteAsync_Should_Return_ActiveTemplates()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedTemplateAsync("AutoComplete Template");

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var results = await CreateQueryRepo(ctx2).GetAutoCompleteAsync(null, 10, CancellationToken.None);

            results.Should().NotBeEmpty();
        }
    }
}
