using BudgetManagement.Infrastructure.Repositories;
using BudgetManagement.Domain.Entities;

namespace BudgetManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterCommandRepository CreateRepository(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static BudgetManagement.Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "MTT001",
            string description = "Test MiscType") =>
            new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            // Delete in FK-dependency order: children first
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.BudgetAllocation");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.BudgetRequest");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.BudgetGroup");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.MiscTypeMaster");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity());

            entity.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity("MTT001", "Test Description"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);

            saved.MiscTypeCode.Should().Be("MTT001");
            saved.Description.Should().Be("Test Description");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toUpdate = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                Id = created.Id,
                MiscTypeCode = "MTT001",
                Description = "Updated Description",
                IsActive = Status.Inactive
            };

            var result = await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("MTT001", "Original"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                Id = created.Id,
                MiscTypeCode = "MTT001",
                Description = "Updated Description",
                IsActive = Status.Inactive
            };

            await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == created.Id);

            updated.Description.Should().Be("Updated Description");
            updated.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var toUpdate = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 9999,
                MiscTypeCode = "MTT999",
                Description = "NonExistent"
            };

            var result = await CreateRepository(ctx).UpdateAsync(9999, toUpdate);

            result.Should().BeFalse();
        }

        // --- DELETE (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteEntity = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(created.Id, deleteEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteEntity = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = IsDelete.Deleted
            };

            await CreateRepository(ctx).DeleteAsync(created.Id, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var deleteEntity = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().BeFalse();
        }
    }
}
