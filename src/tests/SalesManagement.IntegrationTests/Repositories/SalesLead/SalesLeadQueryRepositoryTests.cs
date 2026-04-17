using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesLead;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesLead
{
    [Collection("DatabaseCollection")]
    public sealed class SalesLeadQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesLeadQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesLeadQueryRepository CreateRepo(
            Mock<IPartyLookup>? party = null,
            Mock<ICityLookup>? city = null,
            Mock<IItemLookup>? item = null,
            Mock<IMarketingOfficerAccessFilter>? accessFilter = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>());
                party.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartyLookupDto?)null);
            }
            if (city == null)
            {
                city = new Mock<ICityLookup>(MockBehavior.Loose);
                city.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((CityLookupDto?)null);
            }
            if (item == null)
            {
                item = new Mock<IItemLookup>(MockBehavior.Loose);
                item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());
            }
            if (accessFilter == null)
            {
                accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
                accessFilter.Setup(a => a.IsMarketingOfficer()).Returns(false);
            }

            return new SalesLeadQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object, city.Object, item.Object, accessFilter.Object);
        }

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.EmployeeNo == "SLQ_MO");
            if (existing != null) return existing.Id;

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SLQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SLQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SLQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SLQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var mo = new SalesManagement.Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "SLQ_MO", EmployeeName = "Q Officer",
                MobileNo = "9000000000", Email = "q@y.com",
                Unit = "U", Department = "Sales", Designation = "Mgr",
                SalesOfficeId = office.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MarketingOfficer.AddAsync(mo);
            await ctx.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<int> SeedAsync(string company, string mobile = "9111111111",
            int? partyId = null, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var moId = await EnsureMarketingOfficerAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var l = new SalesManagement.Domain.Entities.SalesLead
            {
                ProspectCompanyName = company,
                ContactName = "C",
                MobileNumber = mobile,
                EmailId = "e@y.com",
                PartyId = partyId,
                MarketingOfficerId = moId,
                InteractionDate = DateTimeOffset.UtcNow,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.SalesLead.AddAsync(l);
            await ctx.SaveChangesAsync();
            return l.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("SLQ_1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("SLQ_UNIQUE_CO", "9112222221");
            await SeedAsync("SLQ_OTHER", "9112222222");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "SLQ_UNIQUE_CO");

            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("SLQ_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("SLQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ProspectCompanyName.Should().Be("SLQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("SLQ_AC1");
            await SeedAsync("SLQ_AC2", "9333333333", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("C", CancellationToken.None);

            result.Count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task MobileNumberExistsForProspectAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("SLQ_MOB", "9444444444");

            var result = await CreateRepo().MobileNumberExistsForProspectAsync("9444444444");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MobileNumberExistsForProspectAsync_Should_Exclude_PartyLeads()
        {
            await ClearAsync();
            await SeedAsync("SLQ_PARTY", "9555555555", partyId: 7);

            var result = await CreateRepo().MobileNumberExistsForProspectAsync("9555555555");

            result.Should().BeFalse(); // PartyId IS NULL filter excludes party-attached leads
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task LeadSourceExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().LeadSourceExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MarketingOfficerExistsAsync_Should_Return_True_For_Active()
        {
            var moId = await EnsureMarketingOfficerAsync();

            var result = await CreateRepo().MarketingOfficerExistsAsync(moId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MarketingOfficerExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().MarketingOfficerExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PartyExistsAsync_Should_Return_True_When_Lookup_Returns_Match()
        {
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 7, PartyName = "P" });

            var result = await CreateRepo(party: party).PartyExistsAsync(7);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var result = await CreateRepo().ItemExistsAsync(9999);
            result.Should().BeFalse();
        }
    }
}
