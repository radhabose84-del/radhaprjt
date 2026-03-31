using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Repositories.SalesGroup;
using SalesManagement.Infrastructure.Repositories.SalesOffice;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesGroup
{
    /// <summary>
    /// Integration tests for SalesGroupQueryRepository.
    /// Verifies Dapper SQL queries (GetAll, GetById, AlreadyExists, NotFound, Autocomplete,
    /// SalesOfficeExistsAsync) against a real SQL Server database.
    /// IInventoryCategoryLookup is mocked to isolate from cross-module dependency.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SalesGroupQueryRepository CreateQueryRepo(Mock<IInventoryCategoryLookup> categoryLookup = null)
        {
            categoryLookup ??= BuildDefaultCategoryLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesGroupQueryRepository(conn, categoryLookup.Object);
        }

        private Mock<IInventoryCategoryLookup> BuildDefaultCategoryLookup(
            int categoryId = 10, string categoryName = "Test Category")
        {
            var mock = new Mock<IInventoryCategoryLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetCategoryByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryMasterDto>
                {
                    new CategoryMasterDto { Id = categoryId, ItemCategoryName = categoryName }
                });
            return mock;
        }

        private async Task ClearTablesAsync()
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOrderHeader");
            await cnn.ExecuteAsync("DELETE FROM Sales.OfficerSalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.MarketingOfficer");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOffice");
            await cnn.ExecuteAsync("DELETE FROM Sales.ItemPriceMaster");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesSegment");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOrganisation");
        }

        /// <summary>
        /// Seeds SalesOrganisation → SalesOffice chain and returns the SalesOfficeId.
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

        private async Task<int> SeedSalesGroupAsync(
            int salesOfficeId,
            string name = "Test Sales Group",
            int? productCategoryId = null,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new SalesGroupCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.SalesGroup
            {
                SalesGroupName = name,
                SalesOfficeId = salesOfficeId,
                ResponsibleManager = "Manager A",
                ProductCategoryId = productCategoryId,
                RegionTerritory = "North",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_PagedResults()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Group Alpha");
            await SeedSalesGroupAsync(officeId, name: "Group Beta");
            await SeedSalesGroupAsync(officeId, name: "Group Gamma");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Alpha Group");
            await SeedSalesGroupAsync(officeId, name: "Beta Group");
            await SeedSalesGroupAsync(officeId, name: "Match Group");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 10, searchTerm: "Match");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].SalesGroupName.Should().Be("Match Group");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId, name: "Deleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.SalesGroupName == "Deleted Group");
        }

        [Fact]
        public async Task GetAllAsync_Should_PopulateProductCategoryName_Via_Lookup()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Lookup Group", productCategoryId: 10);

            var categoryLookup = BuildDefaultCategoryLookup(categoryId: 10, categoryName: "Electronics");
            var repo = CreateQueryRepo(categoryLookup);

            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].ProductCategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetAllAsync_Should_PopulateSalesOfficeName_Via_Join()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Joined Group");

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].SalesOfficeName.Should().Be("Test Office");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination_Page2()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            for (int i = 1; i <= 5; i++)
                await SeedSalesGroupAsync(officeId, name: $"Group {i}");

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Ids = page1.Select(x => x.Id).ToList();
            var page2Ids = page2.Select(x => x.Id).ToList();
            page1Ids.Should().NotIntersectWith(page2Ids);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId, name: "ById Group");

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.SalesGroupName.Should().Be("ById Group");
            dto.SalesOfficeId.Should().Be(officeId);
            dto.SalesOfficeName.Should().Be("Test Office");
        }

        [Fact]
        public async Task GetByIdAsync_Should_PopulateProductCategoryName_Via_Lookup()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId, name: "Cat Group", productCategoryId: 10);

            var categoryLookup = BuildDefaultCategoryLookup(categoryId: 10, categoryName: "Furniture");
            var repo = CreateQueryRepo(categoryLookup);

            var dto = await repo.GetByIdAsync(id);

            dto!.ProductCategoryName.Should().Be("Furniture");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Audit_Fields()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto!.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("test-user");
            dto.CreatedIP.Should().Be("127.0.0.1");
            dto.CreatedDate.Should().NotBeNull();
        }

        // ── AlreadyExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCompositeKeyExists()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Existing Group");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Existing Group", officeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenDoesNotExist()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("NonExistent Group", 99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId, name: "Self Group");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Self Group", officeId, id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenDifferentRecord_HasSameKey()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Duplicate Group");
            var idB = await SeedSalesGroupAsync(officeId, name: "Other Group");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Duplicate Group", officeId, id: idB);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForDeleted_Records()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId, name: "Deleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Deleted Group", officeId);

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Acme Group");
            await SeedSalesGroupAsync(officeId, name: "Acme North");
            await SeedSalesGroupAsync(officeId, name: "XYZ Group");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Acme", CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Group One");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            await SeedSalesGroupAsync(officeId, name: "Active Group", isActive: true);
            await SeedSalesGroupAsync(officeId, name: "Inactive Group", isActive: false);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Group", CancellationToken.None);

            results.Should().NotContain(r => r.SalesGroupName == "Inactive Group");
            results.Should().Contain(r => r.SalesGroupName == "Active Group");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();
            var id = await SeedSalesGroupAsync(officeId, name: "Deleted Auto Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.SalesGroupName == "Deleted Auto Group");
        }

        // ── SalesOfficeExistsAsync ──────────────────────────────────────────

        [Fact]
        public async Task SalesOfficeExistsAsync_Should_Return_True_WhenExists()
        {
            await ClearTablesAsync();
            var officeId = await SeedParentChainAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesOfficeExistsAsync(officeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOfficeExistsAsync_Should_Return_False_WhenNotExists()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesOfficeExistsAsync(99999);

            result.Should().BeFalse();
        }
    }
}
