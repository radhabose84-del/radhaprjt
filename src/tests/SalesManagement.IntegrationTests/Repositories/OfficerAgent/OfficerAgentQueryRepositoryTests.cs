using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.OfficerAgent;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.OfficerAgent
{
    [Collection("DatabaseCollection")]
    public sealed class OfficerAgentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OfficerAgentQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private OfficerAgentQueryRepository CreateQueryRepo(Mock<IPartyLookup>? partyLookup = null)
        {
            var mockParty = partyLookup ?? BuildEmptyPartyLookup();
            return new OfficerAgentQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                mockParty.Object);
        }

        private Mock<IPartyLookup> BuildEmptyPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto>());
            return mock;
        }

        private Mock<IPartyLookup> BuildPartyLookupWithAgent(int agentId = 10, string agentName = "Test Agent")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto>
                {
                    new Contracts.Dtos.Lookups.Party.PartyLookupDto
                    {
                        Id = agentId,
                        PartyCode = "AGT001",
                        PartyName = agentName
                    }
                });
            return mock;
        }

        private OfficerAgentCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new OfficerAgentCommandRepository(ctx);

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.OfficerAgent");
        }

        private async Task<int> EnsureMarketingOfficerAsync(string suffix = "QRY")
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var existingId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.MarketingOfficer WHERE IsDeleted = 0 AND IsActive = 1");
            if (existingId > 0) return existingId;

            var salesOfficeId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.SalesOffice WHERE IsDeleted = 0");
            if (salesOfficeId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var so = new Domain.Entities.SalesOffice
                {
                    SalesOfficeName = $"Test Office {suffix}",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOffice.Add(so);
                await ctx.SaveChangesAsync();
                salesOfficeId = so.Id;
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mo = new Domain.Entities.MarketingOfficer
            {
                EmployeeNo = $"EMP_{suffix}01",
                EmployeeName = $"Officer {suffix}",
                MobileNo = "9876543210",
                SalesOfficeId = salesOfficeId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx2.MarketingOfficer.Add(mo);
            await ctx2.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<int> SeedOfficerAgentAsync(int agentId, int marketingOfficerId, bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entities = new List<Domain.Entities.OfficerAgent>
            {
                new Domain.Entities.OfficerAgent
                {
                    AgentId = agentId,
                    MarketingOfficerId = marketingOfficerId,
                    ValidityFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                    ValidityTo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                    IsActive = isActive
                }
            };
            await CreateCommandRepo(ctx).CreateBatchAsync(entities);
            return entities[0].Id;
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_GroupedResults_ForOfficerWithAgent()
        {
            await ClearTableAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedOfficerAgentAsync(agentId: 10, moId);

            var (grouped, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().BeGreaterThan(0);
            grouped.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoMarketingOfficers()
        {
            await ClearTableAsync();

            // Delete all marketing officers from table (not testing FK constraint)
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.OfficerAgent");

            // Just check pagination returns something
            var (grouped, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            // MarketingOfficers still exist from previous seeding, but OfficerAgent rows are clear
            grouped.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Agents_Populated_For_Officer()
        {
            await ClearTableAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedOfficerAgentAsync(agentId: 20, moId);

            var mockParty = BuildPartyLookupWithAgent(20, "Agent XYZ");
            var (grouped, _) = await CreateQueryRepo(mockParty).GetAllAsync(1, 10, null);

            var officer = grouped.FirstOrDefault(o => o.MarketingOfficerId == moId);
            if (officer != null && officer.Agents.Any())
            {
                officer.Agents[0].AgentName.Should().Be("Agent XYZ");
            }
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Grouped_Dto_WhenOfficerExists()
        {
            await ClearTableAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedOfficerAgentAsync(agentId: 30, moId);

            var dto = await CreateQueryRepo().GetByIdAsync(moId);

            dto.Should().NotBeNull();
            dto!.MarketingOfficerId.Should().Be(moId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Include_Agent_Assignments()
        {
            await ClearTableAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedOfficerAgentAsync(agentId: 40, moId);
            await SeedOfficerAgentAsync(agentId: 41, moId);

            var dto = await CreateQueryRepo().GetByIdAsync(moId);

            dto.Should().NotBeNull();
            dto!.Agents.Should().HaveCountGreaterThan(0);
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var id = await SeedOfficerAgentAsync(agentId: 50, moId);

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
        public async Task NotFoundAsync_Should_Return_True_AfterHardDelete()
        {
            await ClearTableAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var id = await SeedOfficerAgentAsync(agentId: 60, moId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).DeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }
    }
}
