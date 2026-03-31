using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesGroup;
using SalesManagement.Infrastructure.Repositories.SalesOffice;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesGroup
{
    /// <summary>
    /// Integration tests for SalesGroupCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    /// SalesGroup has a same-module FK to SalesOffice (which depends on SalesOrganisation).
    /// Both parent entities are seeded before each test.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesGroupCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesGroupCommandRepository(ctx);

        private Domain.Entities.SalesGroup BuildEntity(
            int salesOfficeId,
            string name = "Test Sales Group",
            string responsibleManager = "Manager A",
            int? productCategoryId = null,
            string regionTerritory = "North")
            => new Domain.Entities.SalesGroup
            {
                SalesGroupName = name,
                SalesOfficeId = salesOfficeId,
                ResponsibleManager = responsibleManager,
                ProductCategoryId = productCategoryId,
                RegionTerritory = regionTerritory,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync()
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOrderHeader");
            await cnn.ExecuteAsync("DELETE FROM Sales.OfficerSalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.MarketingOfficer");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOffice");
        }

        /// <summary>
        /// Seeds a SalesOrganisation → SalesOffice chain and returns the SalesOfficeId.
        /// </summary>
        private async Task<int> SeedParentChainAsync()
        {
            await using var orgCtx = _fixture.CreateFreshDbContext();
            var orgId = await new SalesOrganisationCommandRepository(orgCtx).CreateAsync(
                new Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "ORG" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                    SalesOrganisationName = "Test Org",
                    CompanyId = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            await using var offCtx = _fixture.CreateFreshDbContext();
            var officeId = await new SalesOfficeCommandRepository(offCtx).CreateAsync(
                new Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "Test Office",
                    SalesOrganisationId = orgId,
                    CityId = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            return officeId;
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var newId = await repo.CreateAsync(BuildEntity(officeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_Correct_Fields()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var entity = BuildEntity(officeId,
                name: "Alpha Group",
                responsibleManager: "Mgr Alpha",
                productCategoryId: 10,
                regionTerritory: "West");

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.SalesGroupName.Should().Be("Alpha Group");
            saved.SalesOfficeId.Should().Be(officeId);
            saved.ResponsibleManager.Should().Be("Mgr Alpha");
            saved.ProductCategoryId.Should().Be(10);
            saved.RegionTerritory.Should().Be("West");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_Null_Optional_Fields()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var entity = BuildEntity(officeId,
                name: "Minimal Group",
                responsibleManager: null,
                productCategoryId: null,
                regionTerritory: null);

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ResponsibleManager.Should().BeNull();
            saved.ProductCategoryId.Should().BeNull();
            saved.RegionTerritory.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var newId = await repo.CreateAsync(BuildEntity(officeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Update_Mutable_Fields()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(officeId, name: "Original Group"));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesGroup
            {
                Id = id,
                SalesGroupName = "Updated Group",
                SalesOfficeId = officeId,
                ResponsibleManager = "Mgr Updated",
                ProductCategoryId = 20,
                RegionTerritory = "South",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SalesGroupName.Should().Be("Updated Group");
            saved.ResponsibleManager.Should().Be("Mgr Updated");
            saved.ProductCategoryId.Should().Be(20);
            saved.RegionTerritory.Should().Be("South");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var updated = new Domain.Entities.SalesGroup
            {
                Id = 99999,
                SalesGroupName = "Ghost Group"
            };

            var resultId = await repo.UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(officeId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesGroup
            {
                Id = id,
                SalesGroupName = "Updated Group",
                SalesOfficeId = officeId,
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.Id == id);

            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(officeId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(officeId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesGroup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(officeId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
