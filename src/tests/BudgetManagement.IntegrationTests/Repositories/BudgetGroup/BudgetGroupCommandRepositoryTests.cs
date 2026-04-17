using BudgetManagement.Infrastructure.Repositories.BudgetGroup;
using BudgetManagement.Domain.Entities;

namespace BudgetManagement.IntegrationTests.Repositories.BudgetGroup
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetGroupCommandRepository CreateRepository(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static BudgetManagement.Domain.Entities.BudgetGroup BuildEntity(
            string name = "Test Group",
            int unitId = 1,
            int departmentId = 1) =>
            new BudgetManagement.Domain.Entities.BudgetGroup
            {
                Name = name,
                Description = "Test Budget Group",
                UnitId = unitId,
                DepartmentId = departmentId,
                CostCenterId = 1,
                CurrencyId = 1,
                CarryForward = false,
                IsParent = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Alpha Group", 1, 2), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetGroups.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Name.Should().Be("Alpha Group");
            saved.UnitId.Should().Be(1);
            saved.DepartmentId.Should().Be(2);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetGroups.FirstOrDefaultAsync(x => x.Id == newId);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("Original Name"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.BudgetGroups.FirstAsync(x => x.Id == id);
            entity.Name = "Updated Name";
            entity.Description = "Updated Description";

            await CreateRepository(ctx).UpdateAsync(id, entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BudgetGroups.FirstAsync(x => x.Id == id);

            updated.Name.Should().Be("Updated Name");
            updated.Description.Should().Be("Updated Description");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.BudgetGroups
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_Already_Deleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- EXISTS BY NAME ---

        [Fact]
        public async Task ExistsByNameAndUnitDepartmentAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            await CreateRepository(ctx).CreateAsync(BuildEntity("Unique Group", 1, 1), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var exists = await CreateRepository(ctx).ExistsByNameAndUnitDepartmentAsync("Unique Group", 1, 1, CancellationToken.None);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAndUnitDepartmentAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var exists = await CreateRepository(ctx).ExistsByNameAndUnitDepartmentAsync("NonExistent", 1, 1, CancellationToken.None);

            exists.Should().BeFalse();
        }
    }
}
