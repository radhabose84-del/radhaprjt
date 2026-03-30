using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.UsageType;

namespace InventoryManagement.IntegrationTests.Repositories.UsageType
{
    [Collection("DatabaseCollection")]
    public sealed class UsageTypeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UsageTypeCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UsageTypeCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static InventoryManagement.Domain.Entities.UsageType BuildEntity(
            string code = "USAGE001",
            string name = "Test Usage Type",
            int moduleId = 1) =>
            new InventoryManagement.Domain.Entities.UsageType
            {
                UsageTypeCode = code,
                UsageTypeName = name,
                Description = "Test Description",
                ModuleId = moduleId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UsageType]");
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("USAGE_BLD", "Building Usage", 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UsageType.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.UsageTypeCode.Should().Be("USAGE_BLD");
            saved.UsageTypeName.Should().Be("Building Usage");
            saved.ModuleId.Should().Be(2);
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

            var saved = await ctx.UsageType.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("UPD001", "Original Name"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new InventoryManagement.Domain.Entities.UsageType
            {
                Id = newId,
                UsageTypeCode = "UPD001",
                UsageTypeName = "Updated Name",
                Description = "Updated",
                ModuleId = 1,
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(toUpdate);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("UPD002", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.UsageType
            {
                Id = newId,
                UsageTypeCode = "UPD002",
                UsageTypeName = "Updated",
                Description = "Updated Desc",
                ModuleId = 1,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.UsageType.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.UsageTypeName.Should().Be("Updated");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(newId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("DEL001", "Delete Me"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(newId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.UsageType
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
