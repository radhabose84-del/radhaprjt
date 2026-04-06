using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.CustomerVisit;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.CustomerVisit
{
    [Collection("DatabaseCollection")]
    public sealed class CustomerVisitQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CustomerVisitQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CustomerVisitQueryRepository CreateQueryRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IItemLookup>? itemLookup = null)
        {
            var mockParty = partyLookup ?? BuildDefaultPartyLookup();
            var mockItem = itemLookup ?? BuildEmptyItemLookup();

            return new CustomerVisitQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                mockParty.Object,
                mockItem.Object);
        }

        private Mock<IPartyLookup> BuildDefaultPartyLookup(int partyId = 1, string partyName = "Test Customer")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            var dto = new Contracts.Dtos.Lookups.Party.PartyLookupDto
            {
                Id = partyId,
                PartyCode = "PARTY001",
                PartyName = partyName
            };
            mock.Setup(x => x.GetByIdAsync(partyId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto> { dto });
            return mock;
        }

        private Mock<IItemLookup> BuildEmptyItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());
            return mock;
        }

        private CustomerVisitCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new CustomerVisitCommandRepository(ctx);

        private Domain.Entities.CustomerVisit BuildEntity(
            int customerId = 1,
            int visitTypeId = 1,
            int marketingOfficerId = 1,
            string? remarks = "Test Visit",
            bool isActive = true)
            => new Domain.Entities.CustomerVisit
            {
                CustomerId = customerId,
                VisitTypeId = visitTypeId,
                VisitDateTime = DateTimeOffset.UtcNow,
                Remarks = remarks,
                MarketingOfficerId = marketingOfficerId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.CustomerVisitProduct");
            await conn.ExecuteAsync("DELETE FROM Sales.CustomerVisit");
        }

        private async Task<int> SeedAsync(Domain.Entities.CustomerVisit entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity());

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnRemarks()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity(remarks: "Important Field Visit"));
            await SeedAsync(BuildEntity(customerId: 2, remarks: "Regular Office Visit"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Important");

            total.Should().Be(1);
            items[0].Remarks.Should().Be("Important Field Visit");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CustomerName_From_Lookup()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity(customerId: 1));

            var mockParty = BuildDefaultPartyLookup(1, "XYZ Corp");
            var (items, _) = await CreateQueryRepo(mockParty).GetAllAsync(1, 10, null);

            items[0].CustomerName.Should().Be("XYZ Corp");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 0; i < 5; i++)
                await SeedAsync(BuildEntity(customerId: i + 1, remarks: $"Visit {i}"));

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity(customerId: 1, remarks: "ById Visit"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.CustomerId.Should().Be(1);
            dto.Remarks.Should().Be("ById Visit");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }
    }
}
