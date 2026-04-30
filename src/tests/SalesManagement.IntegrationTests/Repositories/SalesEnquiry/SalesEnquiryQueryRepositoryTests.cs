using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesEnquiry;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesEnquiry
{
    [Collection("DatabaseCollection")]
    public sealed class SalesEnquiryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesEnquiryQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesEnquiryQueryRepository CreateRepo(
            Mock<IPartyLookup>? party = null,
            Mock<IPaymentTermLookup>? paymentTerm = null,
            Mock<IItemLookup>? item = null,
            Mock<IMarketingOfficerAccessFilter>? accessFilter = null,
            Mock<IIPAddressService>? ip = null)
        {
            if (party == null)
            {
                party = new Mock<IPartyLookup>(MockBehavior.Loose);
                party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                            new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            }
            if (paymentTerm == null)
            {
                paymentTerm = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
                paymentTerm.Setup(p => p.GetAllPaymentTermAsync())
                    .ReturnsAsync(new List<PaymentTermLookupDto>
                    {
                        new() { Id = 1, Description = "Net 30" },
                        new() { Id = 2, Description = "Net 60" }
                    });
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
            if (ip == null)
            {
                ip = new Mock<IIPAddressService>(MockBehavior.Loose);
                ip.Setup(x => x.GetUserId()).Returns(1);
            }

            return new SalesEnquiryQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object, paymentTerm.Object, item.Object, accessFilter.Object, ip.Object);
        }

        // Idempotently seeds Sales.MiscTypeMaster (ENQ_TYPE) + Sales.MiscMaster (ENQ_DOMESTIC).
        // Returns the MiscMaster.Id for use in EnquiryTypeId.
        private static async Task<int> EnsureEnqDomesticAsync(ApplicationDbContext ctx)
        {
            var miscType = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(t => t.MiscTypeCode == "ENQ_TYPE" && t.IsDeleted == IsDelete.NotDeleted);
            if (miscType == null)
            {
                miscType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ENQ_TYPE", Description = "Enquiry Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster
                .FirstOrDefaultAsync(m => m.MiscTypeId == miscType.Id && m.Code == "ENQ_DOMESTIC" && m.IsDeleted == IsDelete.NotDeleted);
            if (misc == null)
            {
                misc = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = "ENQ_DOMESTIC", Description = "Domestic", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private async Task<int> SeedAsync(int partyId = 100, string contactPerson = "John",
            int detailCount = 2, IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active,
            string? remarks = "test", int? enquiryTypeId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var enqId = enquiryTypeId ?? await EnsureEnqDomesticAsync(ctx);
            var h = new SalesManagement.Domain.Entities.SalesEnquiryHeader
            {
                PartyId = partyId,
                EnquiryDate = DateTimeOffset.UtcNow,
                EnquiryTypeId = enqId,
                ContactPerson = contactPerson,
                Remarks = remarks,
                IsActive = active, IsDeleted = deleted,
                SalesEnquiryDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.SalesEnquiryDetail
                    {
                        ItemId = i * 10,
                        Quantity = i * 5m,
                        ExmillRate = 100m,
                        TargetPrice = 95m,
                        Discount = 5m
                    }).ToList()
            };
            await ctx.SalesEnquiryHeader.AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync(partyId: 100);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_PartyName_From_Lookup()
        {
            await ClearAsync();
            await SeedAsync(partyId: 100);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows[0].PartyName.Should().Be("Party 100");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync(partyId: 100, deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync(partyId: 1, contactPerson: "UniqueContactXYZ");
            await SeedAsync(partyId: 2, contactPerson: "OtherPerson");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UniqueContactXYZ");

            rows.Should().HaveCount(1);
            rows[0].ContactPerson.Should().Be("UniqueContactXYZ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Details()
        {
            await ClearAsync();
            var id = await SeedAsync(partyId: 5, detailCount: 3);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PartyId.Should().Be(5);
            result.SalesEnquiryDetails.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_ItemName_On_Details()
        {
            await ClearAsync();
            var id = await SeedAsync(detailCount: 2);

            var result = await CreateRepo().GetByIdAsync(id);

            result!.SalesEnquiryDetails[0].ItemName.Should().StartWith("Item");
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
            await SeedAsync(contactPerson: "AutoKeywordZZZ");

            var result = await CreateRepo().AutocompleteAsync("AutoKeywordZZZ", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync(contactPerson: "InactiveKW", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("InactiveKW", CancellationToken.None);

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
            var id = await SeedAsync();

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PartyExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().PartyExistsAsync(100);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PartyExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PartyLookupDto>)new List<PartyLookupDto>());

            var result = await CreateRepo(party: party).PartyExistsAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PaymentTermExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().PaymentTermExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PaymentTermExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().PaymentTermExistsAsync(9999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().ItemExistsAsync(10);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_EnquiryTypeCode_And_Description()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var enqId = await EnsureEnqDomesticAsync(ctx);
            var id = await SeedAsync(enquiryTypeId: enqId);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.EnquiryTypeId.Should().Be(enqId);
            result.EnquiryTypeCode.Should().Be("ENQ_DOMESTIC");
            result.EnquiryTypeDescription.Should().Be("Domestic");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_EnquiryTypeCode_And_Description()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var enqId = await EnsureEnqDomesticAsync(ctx);
            await SeedAsync(enquiryTypeId: enqId);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].EnquiryTypeCode.Should().Be("ENQ_DOMESTIC");
            rows[0].EnquiryTypeDescription.Should().Be("Domestic");
        }

        [Fact]
        public async Task EnquiryTypeExistsAsync_Should_Return_True_For_Active_EnqType_Row()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var enqId = await EnsureEnqDomesticAsync(ctx);

            var result = await CreateRepo().EnquiryTypeExistsAsync(enqId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task EnquiryTypeExistsAsync_Should_Return_False_For_Unknown_Id()
        {
            var result = await CreateRepo().EnquiryTypeExistsAsync(999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnquiryTypeExistsAsync_Should_Return_False_For_MiscMaster_With_Different_Type()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Seed a MiscMaster row under a DIFFERENT MiscType (not ENQ_TYPE)
            var otherType = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(t => t.MiscTypeCode == "TEST_OTHER_TYPE" && t.IsDeleted == IsDelete.NotDeleted);
            if (otherType == null)
            {
                otherType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "TEST_OTHER_TYPE", Description = "Other",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(otherType);
                await ctx.SaveChangesAsync();
            }

            var otherMisc = new SalesManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = otherType.Id, Code = "OTHER_CODE", Description = "Other", SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(otherMisc);
            await ctx.SaveChangesAsync();

            var result = await CreateRepo().EnquiryTypeExistsAsync(otherMisc.Id);

            result.Should().BeFalse();
        }
    }
}
