using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateRepository(NotificationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "MMTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Test MiscType",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private static Domain.Entities.Notification.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "MCODE01",
            string description = "Test MiscMaster") =>
            new()
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(NotificationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MC001", "My Misc"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved.Code.Should().Be("MC001");
            saved.Description.Should().Be("My Misc");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();

            var first = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "SORT01"));
            ctx.ChangeTracker.Clear();
            var second = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "SORT02"));
            ctx.ChangeTracker.Clear();

            var savedFirst = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == first.Id);
            var savedSecond = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == second.Id);

            savedSecond.SortOrder.Should().BeGreaterThan(savedFirst.SortOrder);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
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
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(miscTypeId, "UPD01", "Updated");
            updated.Id = created.Id;
            var result = await CreateRepository(ctx).UpdateAsync(created.Id, updated);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(miscTypeId, "NEWCD", "New Description");
            updated.Id = created.Id;
            updated.SortOrder = 99;
            await CreateRepository(ctx).UpdateAsync(created.Id, updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            saved.Code.Should().Be("NEWCD");
            saved.Description.Should().Be("New Description");
            saved.SortOrder.Should().Be(99);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();

            var entity = BuildEntity(miscTypeId);
            entity.Id = 9999;
            var result = await CreateRepository(ctx).UpdateAsync(9999, entity);

            result.Should().BeFalse();
        }

        // --- DELETE (Soft Delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Notification.MiscMaster { IsDeleted = IsDelete.Deleted };
            var result = await CreateRepository(ctx).DeleteAsync(created.Id, deleteEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Notification.MiscMaster { IsDeleted = IsDelete.Deleted };
            await CreateRepository(ctx).DeleteAsync(created.Id, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.Should().NotBeNull();
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var deleteEntity = new Domain.Entities.Notification.MiscMaster { IsDeleted = IsDelete.Deleted };
            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().BeFalse();
        }

        // --- GET MAX SORT ORDER ---

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Negative_One_When_Empty()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetMaxSortOrderAsync();

            result.Should().Be(-1);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();

            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "SORT01"));
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "SORT02"));

            var result = await CreateRepository(ctx).GetMaxSortOrderAsync();

            result.Should().BeGreaterThan(0);
        }
    }
}
