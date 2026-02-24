#nullable disable
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.BusinessUnit;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.BusinessUnit
{
    /// <summary>
    /// Integration tests for BusinessUnitCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    /// Each test clears the table first to avoid data pollution.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class BusinessUnitCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BusinessUnitCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BusinessUnitCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new BusinessUnitCommandRepository(ctx);

        private Domain.Entities.BusinessUnit BuildEntity(
            string code = "BU001",
            string name = "Test Business Unit",
            string description = "Test Description")
            => new Domain.Entities.BusinessUnit
            {
                BusinessUnitCode = code,
                BusinessUnitName = name,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            ctx.BusinessUnit.RemoveRange(ctx.BusinessUnit);
            await ctx.SaveChangesAsync();
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();

            var newId = await repo.CreateAsync(entity);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_Correct_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity(code: "BU001", name: "Alpha Unit", description: "Alpha Desc");

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.BusinessUnitCode.Should().Be("BU001");
            saved.BusinessUnitName.Should().Be("Alpha Unit");
            saved.Description.Should().Be("Alpha Desc");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_Null_Description()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity(code: "BU002", name: "No Desc Unit", description: null);

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Description.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Update_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity(code: "BU002", name: "Original Name", description: "Original Desc");
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.BusinessUnit
            {
                Id = id,
                BusinessUnitCode = "BU002",   // code preserved (immutable)
                BusinessUnitName = "Updated Name",
                Description = "Updated Desc",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == id);
            saved!.BusinessUnitName.Should().Be("Updated Name");
            saved.Description.Should().Be("Updated Desc");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Clear_Description_When_Null()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity(code: "BU003", name: "Has Desc", description: "Original Desc");
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.BusinessUnit
            {
                Id = id,
                BusinessUnitName = "Has Desc",
                Description = null,
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Description.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity(code: "IMMUTABLE01", name: "Original Name");
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.BusinessUnit
            {
                Id = id,
                BusinessUnitName = "Updated Name",
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == id);
            saved!.BusinessUnitCode.Should().Be("IMMUTABLE01");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);

            var updated = new Domain.Entities.BusinessUnit
            {
                Id = 99999,
                BusinessUnitName = "Ghost Unit"
            };

            var resultId = await repo.UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.BusinessUnit
            {
                Id = id,
                BusinessUnitName = "Updated Name",
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BusinessUnit.FirstOrDefaultAsync(x => x.Id == id);

            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            // Query WITHOUT IsDeleted filter to confirm the flag was set
            var saved = await ctx.BusinessUnit
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);

            var result = await repo.SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            // First delete
            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            // Second delete on same entity (already IsDeleted=Deleted)
            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
