using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        public OfficerAgentQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private OfficerAgentQueryRepository CreateRepo(
            Mock<IPartyLookup>? party = null,
            Mock<IIPAddressService>? ip = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>
                    {
                        new() { Id = 111, PartyName = "Agent A", Mobile = "1234567890" },
                        new() { Id = 222, PartyName = "Agent B", Mobile = "9876543210" }
                    });
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), default))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>
                    {
                        new() { Id = 111, PartyName = "Agent A", Mobile = "1234567890" },
                        new() { Id = 222, PartyName = "Agent B", Mobile = "9876543210" }
                    });
            }
            if (ip == null)
            {
                ip = new Mock<IIPAddressService>(MockBehavior.Loose);
                ip.Setup(x => x.GetEmpId()).Returns((int?)null);
            }

            return new OfficerAgentQueryRepository(
                new SqlConnection(_fixture.ConnectionString), party.Object, ip.Object);
        }

        private async Task<int> EnsureMarketingOfficerAsync(string empNo = "OAQ_MO")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.EmployeeNo == empNo);
            if (existing != null) return existing.Id;

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "OAQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "OAQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "OAQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "OAQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var mo = new SalesManagement.Domain.Entities.MarketingOfficer
            {
                EmployeeNo = empNo,
                EmployeeName = "Officer " + empNo,
                MobileNo = "9876543210",
                Email = "mo@y.com",
                Unit = "U", Department = "Sales", Designation = "Mgr",
                SalesOfficeId = office.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MarketingOfficer.AddAsync(mo);
            await ctx.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<int> SeedAssignmentAsync(int officerId, int agentId, bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new SalesManagement.Domain.Entities.OfficerAgent
            {
                AgentId = agentId,
                MarketingOfficerId = officerId,
                ValidityFrom = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-10)),
                ValidityTo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)),
                IsActive = isActive
            };
            await ctx.OfficerAgent.AddAsync(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Officer_With_Assignments()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedAssignmentAsync(moId, 111);
            await SeedAssignmentAsync(moId, 222);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 50, null);

            total.Should().BeGreaterThan(0);
            var officer = rows.FirstOrDefault(r => r.MarketingOfficerId == moId);
            officer.Should().NotBeNull();
            officer!.Agents.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_Agent_Names_From_Lookup()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedAssignmentAsync(moId, 111);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 50, null);

            var officer = rows.First(r => r.MarketingOfficerId == moId);
            officer.Agents[0].AgentName.Should().Be("Agent A");
            officer.Agents[0].AgentMobile.Should().Be("1234567890");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();

            var (rows, _) = await CreateRepo().GetAllAsync(1, 50, "NoSuchOfficer_XYZ");

            rows.Should().NotContain(r => r.MarketingOfficerId == moId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Officer_Group()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedAssignmentAsync(moId, 111);

            var result = await CreateRepo().GetByIdAsync(moId);

            result.Should().NotBeNull();
            result!.MarketingOfficerId.Should().Be(moId);
            result.Agents.Should().HaveCount(1);
            result.Agents[0].AgentName.Should().Be("Agent A");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_EmpId_Scoped_To_Different_Officer()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetEmpId()).Returns(moId + 1); // scope to a different officer

            var result = await CreateRepo(ip: ip).GetByIdAsync(moId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Officers()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync("OAQ_AC");
            await SeedAssignmentAsync(moId, 111);

            var result = await CreateRepo().AutocompleteAsync("OAQ_AC", CancellationToken.None);

            result.Should().Contain(r => r.MarketingOfficerId == moId);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_When_No_Match()
        {
            var result = await CreateRepo().AutocompleteAsync("NoSuch_ZZZ_XYZ", CancellationToken.None);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var id = await SeedAssignmentAsync(moId, 111);

            var result = await CreateRepo().NotFoundAsync(id);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MarketingOfficerExistsAsync_Should_Return_True()
        {
            var moId = await EnsureMarketingOfficerAsync();
            var result = await CreateRepo().MarketingOfficerExistsAsync(moId);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MarketingOfficerExistsAsync_Should_Return_False_For_Missing()
        {
            var result = await CreateRepo().MarketingOfficerExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AgentExistsAsync_Should_Return_True_When_Lookup_Returns()
        {
            var result = await CreateRepo().AgentExistsAsync(111);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AgentExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>());

            var result = await CreateRepo(party: party).AgentExistsAsync(99999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsExpiredAsync_Should_Return_True_When_ValidityTo_Past()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var expired = new SalesManagement.Domain.Entities.OfficerAgent
            {
                AgentId = 111,
                MarketingOfficerId = moId,
                ValidityFrom = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-2)),
                ValidityTo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
                IsActive = true
            };
            await ctx.OfficerAgent.AddAsync(expired);
            await ctx.SaveChangesAsync();

            var result = await CreateRepo().IsExpiredAsync(expired.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsExpiredAsync_Should_Return_False_When_ValidityTo_Future()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var id = await SeedAssignmentAsync(moId, 111);

            var result = await CreateRepo().IsExpiredAsync(id);

            result.Should().BeFalse();
        }

        // ── AlreadyAssignedAsync (SCRUM-1561) ─────────────────────────────────

        [Fact]
        public async Task AlreadyAssignedAsync_Should_Return_True_When_Active_Row_Exists()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedAssignmentAsync(moId, 111, isActive: true);

            var result = await CreateRepo().AlreadyAssignedAsync(moId, 111);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyAssignedAsync_Should_Return_False_When_Only_Inactive_Rows_Exist()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            await SeedAssignmentAsync(moId, 111, isActive: false);

            var result = await CreateRepo().AlreadyAssignedAsync(moId, 111);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyAssignedAsync_Should_Return_False_When_No_Row_Exists()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();

            var result = await CreateRepo().AlreadyAssignedAsync(moId, 999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyAssignedAsync_Should_Return_False_When_Match_Is_The_Excluded_Row()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var id = await SeedAssignmentAsync(moId, 111, isActive: true);

            // Editing this row's date range should not flag itself as duplicate
            var result = await CreateRepo().AlreadyAssignedAsync(moId, 111, excludeAssignmentId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyAssignedAsync_Should_Return_True_When_Other_Active_Row_Exists_Even_With_Exclude()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var idA = await SeedAssignmentAsync(moId, 111, isActive: true);
            var idB = await SeedAssignmentAsync(moId, 111, isActive: true); // a duplicate that pre-exists in QA-like data

            // Excluding row A still finds row B as an active duplicate
            var result = await CreateRepo().AlreadyAssignedAsync(moId, 111, excludeAssignmentId: idA);

            result.Should().BeTrue();
            idB.Should().NotBe(idA); // sanity
        }

        // ── IsExpiredAndModifiedAsync (SCRUM-1559) ────────────────────────────

        private async Task<(int id, DateOnly from, DateOnly to)> SeedExpiredAssignmentAsync(
            int officerId, int agentId, bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var from = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));
            var to = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)); // expired yesterday
            var e = new SalesManagement.Domain.Entities.OfficerAgent
            {
                AgentId = agentId,
                MarketingOfficerId = officerId,
                ValidityFrom = from,
                ValidityTo = to,
                IsActive = isActive
            };
            await ctx.OfficerAgent.AddAsync(e);
            await ctx.SaveChangesAsync();
            return (e.Id, from, to);
        }

        [Fact]
        public async Task IsExpiredAndModifiedAsync_Should_Return_False_When_Row_NotExpired()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var id = await SeedAssignmentAsync(moId, 111, isActive: true); // future ValidityTo

            // Even with completely different fields, not-expired row never trips the check
            var result = await CreateRepo().IsExpiredAndModifiedAsync(
                id, 999, DateOnly.FromDateTime(DateTime.UtcNow.Date),
                DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(5)), 0);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsExpiredAndModifiedAsync_Should_Return_False_When_Expired_But_Fields_Match()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var (id, from, to) = await SeedExpiredAssignmentAsync(moId, 111, isActive: true);

            // Round-trip: caller sends back exactly what's in DB → no modification → pass
            var result = await CreateRepo().IsExpiredAndModifiedAsync(id, 111, from, to, 1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsExpiredAndModifiedAsync_Should_Return_True_When_Expired_And_ValidityTo_Changed()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var (id, from, _) = await SeedExpiredAssignmentAsync(moId, 111, isActive: true);

            var newTo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)); // try to extend
            var result = await CreateRepo().IsExpiredAndModifiedAsync(id, 111, from, newTo, 1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsExpiredAndModifiedAsync_Should_Return_True_When_Expired_And_IsActive_Changed()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var (id, from, to) = await SeedExpiredAssignmentAsync(moId, 111, isActive: true);

            // User toggles IsActive on an expired row
            var result = await CreateRepo().IsExpiredAndModifiedAsync(id, 111, from, to, 0);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsExpiredAndModifiedAsync_Should_Return_True_When_Expired_And_AgentId_Changed()
        {
            await ClearAsync();
            var moId = await EnsureMarketingOfficerAsync();
            var (id, from, to) = await SeedExpiredAssignmentAsync(moId, 111, isActive: true);

            var result = await CreateRepo().IsExpiredAndModifiedAsync(id, 222, from, to, 1);

            result.Should().BeTrue();
        }
    }
}
