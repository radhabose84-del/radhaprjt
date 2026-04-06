using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.AgentCustomerMapping;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.AgentCustomerMapping
{
    [Collection("DatabaseCollection")]
    public sealed class AgentCustomerMappingQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AgentCustomerMappingQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AgentCustomerMappingQueryRepository CreateQueryRepo(
            Mock<ICustomerLookup>? customerLookup = null,
            Mock<IAgentLookup>? agentLookup = null,
            Mock<ISubAgentLookup>? subAgentLookup = null)
        {
            var mockCustomer = customerLookup ?? BuildDefaultCustomerLookup();
            var mockAgent = agentLookup ?? BuildDefaultAgentLookup();
            var mockSubAgent = subAgentLookup ?? new Mock<ISubAgentLookup>(MockBehavior.Loose);
            mockSubAgent.Setup(x => x.GetAllSubAgentAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.SubAgentLookupDto>());


            return new AgentCustomerMappingQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                mockCustomer.Object,
                mockAgent.Object,
                mockSubAgent.Object);
        }

        private Mock<ICustomerLookup> BuildDefaultCustomerLookup(int customerId = 1, string customerName = "Test Customer")
        {
            var mock = new Mock<ICustomerLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetAllCustomerAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.CustomerLookupDto>
                {
                    new Contracts.Dtos.Lookups.Party.CustomerLookupDto
                    {
                        Id = customerId,
                        CustomerName = customerName,
                        CustomerCode = "CUST001"
                    }
                });
            return mock;
        }

        private Mock<IAgentLookup> BuildDefaultAgentLookup(int agentId = 2, string agentName = "Test Agent")
        {
            var mock = new Mock<IAgentLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetAllAgentAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.AgentLookupDto>
                {
                    new Contracts.Dtos.Lookups.Party.AgentLookupDto
                    {
                        Id = agentId,
                        AgentName = agentName,
                        AgentCode = "AGT001"
                    }
                });
            return mock;
        }

        private AgentCustomerMappingCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new AgentCustomerMappingCommandRepository(ctx);

        private Domain.Entities.AgentCustomerMapping BuildEntity(
            int customerId = 1,
            int agentId = 2,
            int salesSegmentId = 1,
            string? remarks = "Test Remarks",
            bool isActive = true)
            => new Domain.Entities.AgentCustomerMapping
            {
                CustomerId = customerId,
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                EffectiveFrom = DateTime.UtcNow.Date,
                IsDefaultAgent = false,
                Remarks = remarks,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.AgentCustomerMapping");
        }

        private async Task<int> SeedAsync(Domain.Entities.AgentCustomerMapping entity)
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
            await SeedAsync(BuildEntity(remarks: "Special Remarks Alpha"));
            await SeedAsync(BuildEntity(customerId: 2, agentId: 3, remarks: "Other Remarks Beta"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            total.Should().Be(1);
            items[0].Remarks.Should().Be("Special Remarks Alpha");
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

            var mockCustomer = BuildDefaultCustomerLookup(1, "Acme Corp");
            var (items, _) = await CreateQueryRepo(mockCustomer).GetAllAsync(1, 10, null);

            items[0].CustomerName.Should().Be("Acme Corp");
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity(customerId: 1, agentId: 2, remarks: "ById Remarks"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.CustomerId.Should().Be(1);
            dto.AgentId.Should().Be(2);
            dto.Remarks.Should().Be("ById Remarks");
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
