using BudgetManagement.Infrastructure.Repositories;

namespace BudgetManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateCommandRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private MiscTypeMasterCommandRepository CreateMiscTypeRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeAsync(string code = "MT001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await CreateMiscTypeRepo(ctx).CreateAsync(
                new BudgetManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Test Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private BudgetManagement.Domain.Entities.MiscMaster BuildEntity(int miscTypeId, string code = "MSC001") =>
            new BudgetManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "Test MiscMaster",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx)
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
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var entity = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId));

            entity.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var entity = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId, "MSC001"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);

            saved.Code.Should().Be("MSC001");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Auto_Set_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var entity1 = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId, "MSC001"));
            ctx.ChangeTracker.Clear();
            var entity2 = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId, "MSC002"));

            entity2.SortOrder.Should().BeGreaterThan(entity1.SortOrder);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var entity = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var created = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId, "MSC001"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new BudgetManagement.Domain.Entities.MiscMaster
            {
                Id = created.Id,
                MiscTypeId = miscTypeId,
                Code = "MSC001",
                Description = "Updated Description",
                SortOrder = created.SortOrder,
                IsActive = Status.Inactive
            };

            var result = await CreateCommandRepo(ctx).UpdateAsync(created.Id, toUpdate);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var created = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId, "MSC001"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new BudgetManagement.Domain.Entities.MiscMaster
            {
                Id = created.Id,
                MiscTypeId = miscTypeId,
                Code = "MSC001",
                Description = "Updated Description",
                SortOrder = created.SortOrder,
                IsActive = Status.Inactive
            };

            await CreateCommandRepo(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscMaster.FirstAsync(x => x.Id == created.Id);

            updated.Description.Should().Be("Updated Description");
            updated.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var toUpdate = new BudgetManagement.Domain.Entities.MiscMaster
            {
                Id = 9999,
                Code = "MSC999",
                Description = "NonExistent"
            };

            var result = await CreateCommandRepo(ctx).UpdateAsync(9999, toUpdate);

            result.Should().BeFalse();
        }

        // --- DELETE (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var created = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new BudgetManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = IsDelete.Deleted
            };

            var result = await CreateCommandRepo(ctx).DeleteAsync(created.Id, deleteEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var miscTypeId = await SeedMiscTypeAsync();
            var created = await CreateCommandRepo(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new BudgetManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = IsDelete.Deleted
            };

            await CreateCommandRepo(ctx).DeleteAsync(created.Id, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var deleteEntity = new BudgetManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = IsDelete.Deleted
            };

            var result = await CreateCommandRepo(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().BeFalse();
        }
    }
}
