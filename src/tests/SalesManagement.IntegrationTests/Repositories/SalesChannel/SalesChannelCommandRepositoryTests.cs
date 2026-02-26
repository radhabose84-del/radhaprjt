using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesChannel;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesChannel
{
    /// <summary>
    /// Integration tests for SalesChannelCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    /// Each test clears the table first to avoid data pollution.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesChannelCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesChannelCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesChannelCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesChannelCommandRepository(ctx);

        private Domain.Entities.SalesChannel BuildEntity(
            string code = "SC001",
            string name = "Test Sales Channel")
            => new Domain.Entities.SalesChannel
            {
                SalesChannelCode = code,
                SalesChannelName = name,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesItemPriceMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesSegment");
            ctx.SalesChannel.RemoveRange(ctx.SalesChannel);
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
            var entity = BuildEntity(code: "SC001", name: "Alpha Channel");

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesChannel.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.SalesChannelCode.Should().Be("SC001");
            saved.SalesChannelName.Should().Be("Alpha Channel");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
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

            var saved = await ctx.SalesChannel.FirstOrDefaultAsync(x => x.Id == newId);

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
            var entity = BuildEntity(code: "SC002", name: "Original Name");
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesChannel
            {
                Id = id,
                SalesChannelCode = "SC002",   // code preserved (immutable)
                SalesChannelName = "Updated Name",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.SalesChannel.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SalesChannelName.Should().Be("Updated Name");
            saved.IsActive.Should().Be(Status.Inactive);
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

            // Update with a different code — code must remain unchanged
            var updated = new Domain.Entities.SalesChannel
            {
                Id = id,
                SalesChannelName = "Updated Name",
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesChannel.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SalesChannelCode.Should().Be("IMMUTABLE01");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);

            var updated = new Domain.Entities.SalesChannel
            {
                Id = 99999,
                SalesChannelName = "Ghost Channel",
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

            var updated = new Domain.Entities.SalesChannel
            {
                Id = id,
                SalesChannelName = "Updated Name",
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesChannel.FirstOrDefaultAsync(x => x.Id == id);

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
            var saved = await ctx.SalesChannel
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
