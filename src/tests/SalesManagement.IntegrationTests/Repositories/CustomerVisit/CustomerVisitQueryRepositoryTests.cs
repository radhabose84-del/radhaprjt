using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
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
        public CustomerVisitQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CustomerVisitQueryRepository CreateRepo(
            Mock<IPartyLookup>? party = null,
            Mock<IItemLookup>? item = null,
            Mock<IMarketingOfficerAccessFilter>? accessFilter = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>
                    {
                        new() { Id = 1, PartyName = "Customer 1" },
                        new() { Id = 5, PartyName = "Customer 5" }
                    });
                party.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((int id, CancellationToken _) =>
                        id == 99999 ? null : new PartyLookupDto { Id = id, PartyName = "Customer " + id });
            }
            if (item == null)
            {
                item = new Mock<IItemLookup>(MockBehavior.Loose);
                item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                            new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            }
            if (accessFilter == null)
            {
                accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
                accessFilter.Setup(a => a.ShouldApplyFilterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            }

            return new CustomerVisitQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object, item.Object, accessFilter.Object);
        }

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.EmployeeNo == "CVQ_MO");
            if (existing != null) return existing.Id;

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "CVQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "CVQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "CVQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "CVQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var mo = new SalesManagement.Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "CVQ_MO", EmployeeName = "Officer CVQ",
                MobileNo = "9876543210", Email = "mo@y.com",
                Unit = "U", Department = "Sales", Designation = "Mgr",
                SalesOfficeId = office.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MarketingOfficer.AddAsync(mo);
            await ctx.SaveChangesAsync();
            return mo.Id;
        }

        private async Task<int> EnsureVisitTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CVQ_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CVQ_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CVQ_VT");
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "CVQ_VT", Description = "Visit",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedVisitAsync(int customerId = 1, string remarks = "visit", IsDelete deleted = IsDelete.NotDeleted, List<int>? itemIds = null)
        {
            var moId = await EnsureMarketingOfficerAsync();
            var vtId = await EnsureVisitTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var v = new SalesManagement.Domain.Entities.CustomerVisit
            {
                CustomerId = customerId,
                VisitTypeId = vtId,
                VisitDateTime = DateTimeOffset.UtcNow,
                Remarks = remarks,
                MarketingOfficerId = moId,
                IsActive = Status.Active, IsDeleted = deleted
            };
            if (itemIds != null)
            {
                v.CustomerVisitProducts = itemIds.Select(i =>
                    new SalesManagement.Domain.Entities.CustomerVisitProduct { ItemId = i }).ToList();
            }
            await ctx.CustomerVisit.AddAsync(v);
            await ctx.SaveChangesAsync();
            return v.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedVisitAsync(1);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CustomerName_From_Lookup()
        {
            await ClearAsync();
            await SeedVisitAsync(1);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows[0].CustomerName.Should().Be("Customer 1");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedVisitAsync(1, deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm_On_Remarks()
        {
            await ClearAsync();
            await SeedVisitAsync(1, remarks: "SpecialKeyword123");
            await SeedVisitAsync(2, remarks: "other note");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "SpecialKeyword123");

            rows.Should().HaveCount(1);
            rows[0].Remarks.Should().Be("SpecialKeyword123");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedVisitAsync(5, remarks: "byid");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.CustomerId.Should().Be(5);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Load_Products_With_ItemNames()
        {
            await ClearAsync();
            var id = await SeedVisitAsync(1, itemIds: new List<int> { 10, 20 });

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Products.Should().HaveCount(2);
            result.Products.Select(p => p.ItemName).Should().Contain(new[] { "Item 10", "Item 20" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedVisitAsync(1, remarks: "AutoXYZ");

            var result = await CreateRepo().AutocompleteAsync("AutoXYZ", CancellationToken.None);

            result.Should().HaveCount(1);
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
            var id = await SeedVisitAsync(1);

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_True()
        {
            var result = await CreateRepo().CustomerExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_False_When_Not_In_Lookup()
        {
            var result = await CreateRepo().CustomerExistsAsync(99999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task VisitTypeExistsAsync_Should_Return_True_For_Active()
        {
            var vtId = await EnsureVisitTypeAsync();

            var result = await CreateRepo().VisitTypeExistsAsync(vtId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task VisitTypeExistsAsync_Should_Return_False_When_NotFound()
        {
            var result = await CreateRepo().VisitTypeExistsAsync(9999999);
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
        public async Task ItemExistsAsync_Should_Return_True_When_Lookup_Returns()
        {
            var result = await CreateRepo().ItemExistsAsync(10);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());

            var result = await CreateRepo(item: item).ItemExistsAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMarketingOfficerNameAsync_Should_Return_EmployeeName()
        {
            var moId = await EnsureMarketingOfficerAsync();

            var result = await CreateRepo().GetMarketingOfficerNameAsync(moId);

            result.Should().Be("Officer CVQ");
        }

        [Fact]
        public async Task GetMarketingOfficerNameAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetMarketingOfficerNameAsync(9999999);
            result.Should().BeNull();
        }
    }
}
