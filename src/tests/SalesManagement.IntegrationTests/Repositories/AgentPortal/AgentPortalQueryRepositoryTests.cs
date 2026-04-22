using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.AgentPortal;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.AgentPortal
{
    /// <summary>
    /// Integration tests for AgentPortalQueryRepository (Dapper-based, read-only).
    ///
    /// AgentPortalQueryRepository provides agent-scoped views over AgentCustomerMapping,
    /// SalesEnquiryHeader, SalesOrderHeader, InvoiceHeader, ComplaintHeader, and DispatchAdviceHeader.
    ///
    /// Cross-module lookups (IPartyLookup, IItemLookup) are mocked.
    /// Same-module FK data (SalesGroup, MiscMaster) is seeded via EnsurePrerequisitesAsync.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class AgentPortalQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AgentPortalQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private AgentPortalQueryRepository CreateRepo()
        {
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyCode = "P" + id, PartyName = "Party " + id }).ToList());

            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());

            return new AgentPortalQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object,
                item.Object);
        }

        // ── Seed helpers ──────────────────────────────────────────────────

        private async Task SeedAgentCustomerMappingAsync(
            int agentId, int customerId,
            bool isActive = true, bool isDeleted = false,
            DateOnly? effectiveTo = null)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                INSERT INTO Sales.AgentCustomerMapping
                    (AgentId, CustomerId, SalesGroupId, IsDefaultAgent, EffectiveFrom, EffectiveTo, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@AgentId, @CustomerId, @GroupId, 1, GETDATE(), @EffectiveTo, @IsActive, @IsDeleted,
                     1, GETDATE(), 'test-user', '127.0.0.1')",
                new
                {
                    AgentId = agentId,
                    CustomerId = customerId,
                    GroupId = _salesGroupId,
                    IsActive = isActive ? 1 : 0,
                    IsDeleted = isDeleted ? 1 : 0,
                    EffectiveTo = effectiveTo?.ToDateTime(TimeOnly.MinValue)
                });
        }

        private int _salesGroupId = 1;

        private async Task EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var org = await ctx.SalesOrganisation.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationCode == "APORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "APORG", SalesOrganisationName = "AP Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var office = await ctx.SalesOffice.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationId == org.Id && x.SalesOfficeName == "AP Office");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "AP Office",
                    SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var group = await ctx.SalesGroup.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOfficeId == office.Id && x.SalesGroupName == "AP_GRP");
            if (group == null)
            {
                group = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "AP_GRP",
                    SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(group);
                await ctx.SaveChangesAsync();
            }

            _salesGroupId = group.Id;
        }

        private async Task ClearAsync()
        {
            await _fixture.ClearTablesAsync("Sales.AgentCustomerMapping");
            await EnsurePrerequisitesAsync();
        }

        // ── GetAgentCustomerIdsAsync ──────────────────────────────────────

        [Fact]
        public async Task GetAgentCustomerIdsAsync_Should_Return_Active_Mappings()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(agentId: 1, customerId: 100);
            await SeedAgentCustomerMappingAsync(agentId: 1, customerId: 200);

            var ids = await CreateRepo().GetAgentCustomerIdsAsync(1);

            ids.Should().HaveCount(2);
            ids.Should().Contain(100);
            ids.Should().Contain(200);
        }

        [Fact]
        public async Task GetAgentCustomerIdsAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(agentId: 2, customerId: 300, isActive: false);

            var ids = await CreateRepo().GetAgentCustomerIdsAsync(2);

            ids.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAgentCustomerIdsAsync_Should_Exclude_Deleted()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(agentId: 3, customerId: 400, isDeleted: true);

            var ids = await CreateRepo().GetAgentCustomerIdsAsync(3);

            ids.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAgentCustomerIdsAsync_Should_Exclude_Expired()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(
                agentId: 4, customerId: 500,
                effectiveTo: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)));

            var ids = await CreateRepo().GetAgentCustomerIdsAsync(4);

            ids.Should().BeEmpty();
        }

        // ── GetDashboardAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetDashboardAsync_Should_Return_ZeroCounts_When_NoCustomers()
        {
            var result = await CreateRepo().GetDashboardAsync(9999, new List<int>());

            result.Should().NotBeNull();
            result.TotalCustomers.Should().Be(0);
            result.OpenEnquiries.Should().Be(0);
            result.ActiveOrders.Should().Be(0);
        }

        [Fact]
        public async Task GetDashboardAsync_Should_Return_CustomerCount()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(agentId: 10, customerId: 1001);
            await SeedAgentCustomerMappingAsync(agentId: 10, customerId: 1002);

            var result = await CreateRepo().GetDashboardAsync(10, new List<int> { 1001, 1002 });

            result.TotalCustomers.Should().Be(2);
        }

        // ── GetMyCustomersAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetMyCustomersAsync_Should_Return_Paginated_Results()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(agentId: 20, customerId: 2001);
            await SeedAgentCustomerMappingAsync(agentId: 20, customerId: 2002);
            await SeedAgentCustomerMappingAsync(agentId: 20, customerId: 2003);

            var (data, total) = await CreateRepo().GetMyCustomersAsync(20, 1, 2, null);

            total.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMyCustomersAsync_Should_Populate_PartyNames_Via_Lookup()
        {
            await ClearAsync();
            await SeedAgentCustomerMappingAsync(agentId: 21, customerId: 3001);

            var (data, _) = await CreateRepo().GetMyCustomersAsync(21, 1, 10, null);

            data.Should().HaveCount(1);
            data[0].CustomerName.Should().Be("Party 3001");
            data[0].CustomerCode.Should().Be("P3001");
        }

        // ── GetEnquiriesAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetEnquiriesAsync_Should_Return_Empty_When_NoCustomerIds()
        {
            var (data, total) = await CreateRepo().GetEnquiriesAsync(
                new List<int>(), 1, 10, null);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetSalesOrdersAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetSalesOrdersAsync_Should_Return_Empty_When_AgentIdZero()
        {
            var (data, total) = await CreateRepo().GetSalesOrdersAsync(
                0, 1, 10, null);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetComplaintsAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetComplaintsAsync_Should_Return_Empty_When_NoCustomerIds()
        {
            var (data, total) = await CreateRepo().GetComplaintsAsync(
                new List<int>(), 1, 10, null);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetInvoicesAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetInvoicesAsync_Should_Return_Empty_When_NoCustomerIds()
        {
            var (data, total) = await CreateRepo().GetInvoicesAsync(
                new List<int>(), 1, 10, null);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetDispatchesAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetDispatchesAsync_Should_Return_Empty_When_NoCustomerIds()
        {
            var (data, total) = await CreateRepo().GetDispatchesAsync(
                new List<int>(), 1, 10, null);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetCommissionsAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetCommissionsAsync_Should_Return_Empty_When_NoConfig()
        {
            var result = await CreateRepo().GetCommissionsAsync(9999);

            result.Should().BeEmpty();
        }
    }
}
