using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.FeederGroup
{
    [Collection("DatabaseCollection")]
    public sealed class FeederGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FeederGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private FeederGroupCommandRepository CreateRepo(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.Power.FeederGroup BuildEntity(
            string code, string name) =>
            new MaintenanceManagement.Domain.Entities.Power.FeederGroup
            {
                FeederGroupCode = code,
                FeederGroupName = name,
                UnitId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_C1", "Create Group 1"));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_C2", "Create Group 2"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FeederGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.FeederGroupCode.Should().Be("FG_C2");
            saved.FeederGroupName.Should().Be("Create Group 2");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_C3", "Create Group 3"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FeederGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_U1", "Orig"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(id, BuildEntity("FG_U1", "Updated"));

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_U2", "Before"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(id, BuildEntity("FG_U2", "After Update"));
            ctx.ChangeTracker.Clear();

            var updated = await ctx.FeederGroup.FirstOrDefaultAsync(x => x.Id == id);
            updated!.FeederGroupName.Should().Be("After Update");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepo(ctx).UpdateAsync(99999, BuildEntity("FG_NA", "Not exist"));

            result.Should().BeFalse();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_D1", "Delete Me"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("FG_D2", "Soft Delete"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.FeederGroup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepo(ctx).DeleteAsync(99999,
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
