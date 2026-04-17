using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace InventoryManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static InventoryManagement.Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "INV_TYPE",
            string description = "Inventory Type") =>
            new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) => await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity("INV_TYPE", "Inventory Type"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved.MiscTypeCode.Should().Be("INV_TYPE");
            saved.Description.Should().Be("Inventory Type");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE001", "Original"));
            ctx.ChangeTracker.Clear();

            created.Description = "Updated Description";
            var result = await CreateRepository(ctx).UpdateAsync(created.Id, created);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE001", "Original"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                Id = created.Id,
                MiscTypeCode = "CODE001",
                Description = "Updated Description",
                IsActive = BaseEntity.Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 9999,
                MiscTypeCode = "NOTEXIST",
                Description = "Not Found",
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(9999, entity);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var toDelete = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, toDelete);

            result.Should().BeFalse();
        }
    }
}
