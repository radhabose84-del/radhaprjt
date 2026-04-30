using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesContact;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesContact
{
    [Collection("DatabaseCollection")]
    public sealed class SalesContactQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesContactQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesContactQueryRepository CreateRepo(
            Mock<IPartyLookup>? party = null,
            Mock<IMarketingOfficerAccessFilter>? accessFilter = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>());
            }
            if (accessFilter == null)
            {
                accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
                accessFilter.Setup(a => a.ShouldApplyFilterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            }

            return new SalesContactQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object, accessFilter.Object, _fixture.IpMock.Object);
        }

        private async Task<int> EnsureContactTypeAsync(string code = "SCQ_PRIM")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SCQ_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SCQ_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedAsync(
            string name, string mobile, int? partyId = 1,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var typeId = await EnsureContactTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var c = new SalesManagement.Domain.Entities.SalesContact
            {
                ContactName = name,
                MobileNumber = mobile,
                ContactTypeId = typeId,
                PartyId = partyId,
                Email = "x@y.com",
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.SalesContact.AddAsync(c);
            await ctx.SaveChangesAsync();
            return c.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("SCQ_1", "1100000001");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("SCQ_UNIQ", "1200000001");
            await SeedAsync("SCQ_OTHER", "1200000002");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "SCQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].ContactName.Should().Be("SCQ_UNIQ");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("SCQ_DEL", "1300000001", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("SCQ_GBI", "1400000001");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ContactName.Should().Be("SCQ_GBI");
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
            await SeedAsync("SCQ_AC1", "1500000001");
            await SeedAsync("SCQ_AC2", "1500000002", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("SCQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task MobileAlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("SCQ_MOB", "1600000001");

            var result = await CreateRepo().MobileAlreadyExistsAsync("1600000001");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MobileAlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("SCQ_MOBSELF", "1700000001");

            var result = await CreateRepo().MobileAlreadyExistsAsync("1700000001", excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ContactTypeExistsAsync_Should_Return_True_For_Active()
        {
            var typeId = await EnsureContactTypeAsync();

            var result = await CreateRepo().ContactTypeExistsAsync(typeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ContactTypeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().ContactTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_MarketingOfficer_Should_Filter_By_AccessibleCustomers()
        {
            await ClearAsync();
            await SeedAsync("SCQ_MO1", "1800000001", partyId: 5);
            await SeedAsync("SCQ_MO2", "1800000002", partyId: 99);
            var accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
            accessFilter.Setup(a => a.ShouldApplyFilterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            accessFilter.Setup(a => a.GetAccessibleCustomerIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<int>)new List<int> { 5 });

            var (rows, _) = await CreateRepo(accessFilter: accessFilter).GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].PartyId.Should().Be(5);
        }
    }
}
