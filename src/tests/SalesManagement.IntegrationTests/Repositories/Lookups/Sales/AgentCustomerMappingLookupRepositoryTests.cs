using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class AgentCustomerMappingLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AgentCustomerMappingLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AgentCustomerMappingLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AgentCustomerMappingLookupRepository(conn);
        }

        private async Task SeedAsync(
            int agentId,
            int customerId,
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.AgentCustomerMapping NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.AgentCustomerMapping
                    (AgentId, CustomerId, SalesGroupId, EffectiveFrom, EffectiveTo,
                     IsDefaultAgent, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@AgentId, @CustomerId, 1, @EffectiveFrom, @EffectiveTo,
                     0, @IsActive, @IsDeleted, 1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.AgentCustomerMapping CHECK CONSTRAINT ALL;",
                new
                {
                    AgentId = agentId,
                    CustomerId = customerId,
                    EffectiveFrom = effectiveFrom ?? DateTime.UtcNow.AddDays(-30),
                    EffectiveTo = effectiveTo,
                    IsActive = isActive,
                    IsDeleted = isDeleted
                });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("Sales.AgentCustomerMapping");

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Returns_Matching_Customers()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);
            await SeedAsync(agentId: 10, customerId: 101);
            await SeedAsync(agentId: 20, customerId: 200);

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(10);

            result.Should().BeEquivalentTo(new[] { 100, 101 });
        }

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);
            await SeedAsync(agentId: 10, customerId: 101, isDeleted: true);

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(10);

            result.Should().BeEquivalentTo(new[] { 100 });
        }

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Excludes_Inactive()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);
            await SeedAsync(agentId: 10, customerId: 101, isActive: false);

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(10);

            result.Should().BeEquivalentTo(new[] { 100 });
        }

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Excludes_FutureEffectiveFrom()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);
            await SeedAsync(agentId: 10, customerId: 101, effectiveFrom: DateTime.UtcNow.AddDays(30));

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(10);

            result.Should().BeEquivalentTo(new[] { 100 });
        }

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Excludes_PastEffectiveTo()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);
            await SeedAsync(agentId: 10, customerId: 101, effectiveTo: DateTime.UtcNow.AddDays(-1));

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(10);

            result.Should().BeEquivalentTo(new[] { 100 });
        }

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Deduplicates_Customers()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);
            await SeedAsync(agentId: 10, customerId: 100);

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(10);

            result.Should().ContainSingle().Which.Should().Be(100);
        }

        [Fact]
        public async Task GetCustomerIdsByAgentAsync_Returns_Empty_When_NoMatches()
        {
            await ClearAsync();
            await SeedAsync(agentId: 10, customerId: 100);

            var result = await CreateRepo().GetCustomerIdsByAgentAsync(999);

            result.Should().BeEmpty();
        }
    }
}
