using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.IntegrationTests.Repositories.Templates
{
    [Collection("DatabaseCollection")]
    public sealed class TemplateCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TemplateCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private TemplateCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static InspectionTemplate BuildEntity(string name = "Test Template") =>
            new InspectionTemplate
            {
                TemplateName = name,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[InspectionParameter]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[InspectionTemplate]");
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_TemplateName()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("Quality Check Template"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.InspectionTemplate.FirstOrDefaultAsync(x => x.Id == id);
            saved.Should().NotBeNull();
            saved!.TemplateName.Should().Be("Quality Check Template");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.InspectionTemplate.FirstOrDefaultAsync(x => x.Id == id);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }
    }
}
