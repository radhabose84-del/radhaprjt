using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
            Mock<IUserLookup> userLookup = null,
            Mock<IOfficerAgentUserLookup> officerAgentUserLookup = null,
            Mock<IDataAccessFilter>? accessFilter = null)
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
            if (userLookup == null)
            {
                userLookup = new Mock<IUserLookup>(MockBehavior.Loose);
                userLookup.Setup(u => u.GetAllUserAsync())
                    .ReturnsAsync(new List<UserLookupDto>());
            }
            if (officerAgentUserLookup == null)
            {
                officerAgentUserLookup = new Mock<IOfficerAgentUserLookup>(MockBehavior.Loose);
            }
            if (accessFilter == null)
            {
                accessFilter = new Mock<IDataAccessFilter>(MockBehavior.Loose);
                accessFilter.Setup(a => a.GetContextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DataAccessContext.Unrestricted);
            }

            return new AgentCustomerMappingQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                customer.Object, agent.Object, subAgent.Object,
                userLookup.Object, officerAgentUserLookup.Object, accessFilter.Object);
        }

        private async Task<int> EnsureSalesGroupAsync()
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

            var office = await ctx.SalesOffice.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationId == org.Id);
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "ACMQ Office",
                    SalesOrganisationId = org.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var existing = await ctx.SalesGroup.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOfficeId == office.Id);
            if (existing != null) return existing.Id;

            var g = new SalesManagement.Domain.Entities.SalesGroup
            {
                SalesGroupName = "ACMQ Group",
                SalesOfficeId = office.Id,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.SalesGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<int> SeedAsync(int customerId, int agentId,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var grpId = await EnsureSalesGroupAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = new SalesManagement.Domain.Entities.AgentCustomerMapping
            {
                CustomerId = customerId,
                AgentId = agentId,
                SalesGroupId = grpId,
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
        public async Task SalesGroupExistsAsync_Should_Return_True_For_Active()
        {
            var grpId = await EnsureSalesGroupAsync();

            var result = await CreateRepo().SalesGroupExistsAsync(grpId);

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
