using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceCategory;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceCategory
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceCategoryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceCategoryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceCategoryCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.MaintenanceCategory BuildEntity(
            string categoryName = "Electrical",
            string description = "Electrical maintenance") =>
            new MaintenanceManagement.Domain.Entities.MaintenanceCategory
            {
                CategoryName = categoryName,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MaintenanceCategory]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Mechanical", "Mechanical systems"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MaintenanceCategory.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CategoryName.Should().Be("Mechanical");
            saved.Description.Should().Be("Mechanical systems");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MaintenanceCategory.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Value_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Civil", "Civil works"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory
                {
                    Id = newId,
                    CategoryName = "Civil Updated",
                    Description = "Civil updated desc",
                    IsActive = BaseEntity.Status.Active
                });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("HVAC", "HVAC systems"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory
                {
                    Id = newId,
                    CategoryName = "HVAC Updated",
                    Description = "Updated HVAC",
                    IsActive = BaseEntity.Status.Active
                });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MaintenanceCategory.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.CategoryName.Should().Be("HVAC Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory
                {
                    Id = 9999,
                    CategoryName = "NotFound",
                    IsActive = BaseEntity.Status.Active
                });

            result.Should().BeLessThan(0);
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Value_GreaterThanZero_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Plumbing", "Plumbing systems"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MaintenanceCategory
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
