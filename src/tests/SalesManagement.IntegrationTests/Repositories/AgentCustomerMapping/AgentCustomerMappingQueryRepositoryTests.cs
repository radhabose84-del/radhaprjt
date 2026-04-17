using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
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
        public AgentCustomerMappingQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private AgentCustomerMappingQueryRepository CreateRepo(
            Mock<ICustomerLookup>? customer = null,
            Mock<IAgentLookup>? agent = null,
            Mock<ISubAgentLookup>? subAgent = null,
            Mock<IMarketingOfficerAccessFilter>? accessFilter = null)
        {
            if (customer == null)
            {
                customer = new Mock<ICustomerLookup>(MockBehavior.Loose);
                customer.Setup(c => c.GetAllCustomerAsync())
                    .ReturnsAsync((IReadOnlyList<CustomerLookupDto>)new List<CustomerLookupDto>
                    {
                        new() { Id = 1, CustomerName = "C1" }
                    });
            }
            if (agent == null)
            {
                agent = new Mock<IAgentLookup>(MockBehavior.Loose);
                agent.Setup(a => a.GetAllAgentAsync())
                    .ReturnsAsync((IReadOnlyList<AgentLookupDto>)new List<AgentLookupDto>
                    {
                        new() { Id = 10, AgentName = "A1" }
                    });
            }
            if (subAgent == null)
            {
                subAgent = new Mock<ISubAgentLookup>(MockBehavior.Loose);
                subAgent.Setup(s => s.GetAllSubAgentAsync())
                    .ReturnsAsync((IReadOnlyList<SubAgentLookupDto>)new List<SubAgentLookupDto>());
            }
            if (accessFilter == null)
            {
                accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
                accessFilter.Setup(a => a.IsMarketingOfficer()).Returns(false);
            }

            return new AgentCustomerMappingQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                customer.Object, agent.Object, subAgent.Object, accessFilter.Object);
        }

        private async Task<int> EnsureSegmentAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationCode == "ACMQSO");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "ACMQSO", SalesOrganisationName = "ACMQ Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var ch = await ctx.SalesChannel.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesChannelCode == "ACMQSC");
            if (ch == null)
            {
                ch = new SalesManagement.Domain.Entities.SalesChannel
                {
                    SalesChannelCode = "ACMQSC", SalesChannelName = "ACMQ Channel",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesChannel.AddAsync(ch);
                await ctx.SaveChangesAsync();
            }

            var bu = await ctx.BusinessUnit.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.BusinessUnitCode == "ACMQBU");
            if (bu == null)
            {
                bu = new SalesManagement.Domain.Entities.BusinessUnit
                {
                    BusinessUnitCode = "ACMQBU", BusinessUnitName = "ACMQ BU",
                    Description = "ACMQ BU", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.BusinessUnit.AddAsync(bu);
                await ctx.SaveChangesAsync();
            }

            var existing = await ctx.SalesSegment.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationId == org.Id && x.SalesChannelId == ch.Id && x.BusinessUnitId == bu.Id);
            if (existing != null) return existing.Id;

            var s = new SalesManagement.Domain.Entities.SalesSegment
            {
                SalesOrganisationId = org.Id,
                SalesChannelId = ch.Id,
                BusinessUnitId = bu.Id,
                SegmentName = "ACMQ Seg",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.SalesSegment.AddAsync(s);
            await ctx.SaveChangesAsync();
            return s.Id;
        }

        private async Task<int> SeedAsync(int customerId, int agentId,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var segId = await EnsureSegmentAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = new SalesManagement.Domain.Entities.AgentCustomerMapping
            {
                CustomerId = customerId,
                AgentId = agentId,
                SalesSegmentId = segId,
                EffectiveFrom = DateTime.UtcNow.Date,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.AgentCustomerMapping.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("Sales.AgentCustomerMapping");

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync(1, 10);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync(1, 10, deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync(5, 50);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CustomerId.Should().Be(5);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByCustomerIdAsync_Should_Return_Matching_Mappings()
        {
            await ClearAsync();
            await SeedAsync(7, 70);
            await SeedAsync(7, 80);
            await SeedAsync(8, 90);

            var result = await CreateRepo().GetByCustomerIdAsync(7);

            result.Should().HaveCount(2);
            result.Should().AllSatisfy(r => r.CustomerId.Should().Be(7));
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().CustomerExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_False_When_Not_In_Lookup()
        {
            var result = await CreateRepo().CustomerExistsAsync(9999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AgentExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().AgentExistsAsync(10);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesSegmentExistsAsync_Should_Return_True_For_Active()
        {
            var segId = await EnsureSegmentAsync();

            var result = await CreateRepo().SalesSegmentExistsAsync(segId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MappingAlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync(20, 200);

            var result = await CreateRepo().MappingAlreadyExistsAsync(20, 200);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MappingAlreadyExistsAsync_Should_Return_False_When_Different_Pair()
        {
            await ClearAsync();
            await SeedAsync(21, 210);

            var result = await CreateRepo().MappingAlreadyExistsAsync(21, 999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_Placeholder()
        {
            // Placeholder implementation always returns false until transaction tables reference this entity.
            await ClearAsync();
            var id = await SeedAsync(22, 220);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
