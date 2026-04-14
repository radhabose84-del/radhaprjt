using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrganisation
{
    /// <summary>
    /// Integration tests for SalesOrganisationCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    /// Each test clears the table first to avoid data pollution.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesOrganisationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesOrganisationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesOrganisationCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesOrganisationCommandRepository(ctx);

        private Domain.Entities.SalesOrganisation BuildEntity(
            string code = "SO001",
            string name = "Test Sales Org",
            int companyId = 1,
            string description = "Test Description")
            => new Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = code,
                SalesOrganisationName = name,
                CompanyId = companyId,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
                DELETE FROM Sales.ItemPriceMaster;
                DELETE FROM Sales.SalesOffice;
                DELETE FROM Sales.SalesSegment;
                DELETE FROM Sales.SalesOrganisation;");
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
            var entity = BuildEntity(code: "SO001", name: "Alpha Org", companyId: 10, description: "Alpha Desc");

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.SalesOrganisationCode.Should().Be("SO001");
            saved.SalesOrganisationName.Should().Be("Alpha Org");
            saved.CompanyId.Should().Be(10);
            saved.Description.Should().Be("Alpha Desc");
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

            var saved = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.Id == newId);

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
            var entity = BuildEntity(code: "SO002", name: "Original Name", companyId: 1, description: "Original Desc");
            var id = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesOrganisation
            {
                Id = id,
                SalesOrganisationCode = "SO002",   // code preserved (immutable)
                SalesOrganisationName = "Updated Name",
                CompanyId = 2,
                Description = "Updated Desc",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SalesOrganisationName.Should().Be("Updated Name");
            saved.CompanyId.Should().Be(2);
            saved.Description.Should().Be("Updated Desc");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);

            var updated = new Domain.Entities.SalesOrganisation
            {
                Id = 99999,
                SalesOrganisationName = "Ghost Org",
                CompanyId = 1
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

            var updated = new Domain.Entities.SalesOrganisation
            {
                Id = id,
                SalesOrganisationName = "Updated Name",
                CompanyId = 1,
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.Id == id);

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
            var saved = await ctx.SalesOrganisation
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
