#nullable disable
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MiscMaster;
using SalesManagement.Infrastructure.Repositories.MiscTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MiscMaster
{
    /// <summary>
    /// Integration tests for MiscMasterCommandRepository.
    /// Verifies EF Core Create, Update, SoftDelete, and GetMaxSortOrder operations.
    /// MiscTypeMaster rows are seeded as FK prerequisites; MiscMaster table is cleared before each test.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new MiscMasterCommandRepository(ctx);

        private MiscTypeMasterCommandRepository CreateTypeRepository(ApplicationDbContext ctx)
            => new MiscTypeMasterCommandRepository(ctx);

        private Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "CODE001",
            string description = "Test Misc Master",
            int sortOrder = 1) =>
            new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearMiscMasterTableAsync(ApplicationDbContext ctx)
        {
            ctx.MiscMaster.RemoveRange(ctx.MiscMaster);
            await ctx.SaveChangesAsync();
        }

        /// <summary>Seeds a MiscTypeMaster row if none exists for the given code; returns its Id.</summary>
        private async Task<int> EnsureMiscTypeExistsAsync(string code = "MISC001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(x => x.MiscTypeCode == code && x.IsDeleted == IsDelete.NotDeleted);

            if (existing != null)
                return existing.Id;

            return await CreateTypeRepository(ctx).CreateAsync(new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = $"Fixture Misc Type {code}",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity(miscTypeId, code: "CODE001", description: "Test Item", sortOrder: 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MiscTypeId.Should().Be(miscTypeId);
            saved.Code.Should().Be("CODE001");
            saved.Description.Should().Be("Test Item");
            saved.SortOrder.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Return_UpdatedId()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, code: "CODE002", description: "Original"));
            ctx.ChangeTracker.Clear();

            var updatedId = await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Updated",
                SortOrder = 10,
                IsActive = Status.Active
            });

            updatedId.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, code: "CODE003", description: "Original", sortOrder: 1));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Updated Description",
                SortOrder = 20,
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Description.Should().Be("Updated Description");
            saved.SortOrder.Should().Be(20);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, code: "IMMUTABLE01", description: "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Changed Description",
                SortOrder = 1,
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Code.Should().Be("IMMUTABLE01");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = 99999,
                Description = "Ghost",
                SortOrder = 1,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Modified",
                SortOrder = 1,
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── GetMaxSortOrderAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_WhenNoRecords()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync("MAXSORT01");
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            var result = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_MaxSortOrder_ForMiscType()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync("MAXSORT02");
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "S001", "Item 1", sortOrder: 3));
            ctx.ChangeTracker.Clear();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "S002", "Item 2", sortOrder: 7));
            ctx.ChangeTracker.Clear();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "S003", "Item 3", sortOrder: 5));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            result.Should().Be(7);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Not_Count_SoftDeleted_Records()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync("MAXSORT03");
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "S010", "Item High", sortOrder: 99));
            ctx.ChangeTracker.Clear();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "S011", "Item Low", sortOrder: 2));
            ctx.ChangeTracker.Clear();

            // Soft delete the high-sort-order item
            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            result.Should().Be(2);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Be_Scoped_To_MiscTypeId()
        {
            var miscTypeId1 = await EnsureMiscTypeExistsAsync("SCOPE01");
            var miscTypeId2 = await EnsureMiscTypeExistsAsync("SCOPE02");
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearMiscMasterTableAsync(ctx);

            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId1, "SC001", "Type1 Item", sortOrder: 10));
            ctx.ChangeTracker.Clear();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId2, "SC002", "Type2 Item", sortOrder: 50));
            ctx.ChangeTracker.Clear();

            var max1 = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId1);
            var max2 = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId2);

            max1.Should().Be(10);
            max2.Should().Be(50);
        }
    }
}
