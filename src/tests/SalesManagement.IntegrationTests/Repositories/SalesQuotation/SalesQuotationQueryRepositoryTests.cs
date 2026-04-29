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
using SalesManagement.Infrastructure.Repositories.SalesQuotation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesQuotation
{
    [Collection("DatabaseCollection")]
    public sealed class SalesQuotationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesQuotationQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesQuotationQueryRepository CreateRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IPaymentTermLookup>? paymentTermLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<IHSNLookup>? hsnLookup = null,
            Mock<IUOMLookup>? uomLookup = null,
            Mock<IMarketingOfficerAccessFilter>? accessFilter = null,
            Mock<IIPAddressService>? ip = null)
        {
            if (partyLookup == null)
            {
                partyLookup = new Mock<IPartyLookup>(MockBehavior.Loose);
                partyLookup.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                            new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            }
            if (paymentTermLookup == null)
            {
                paymentTermLookup = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
                paymentTermLookup.Setup(p => p.GetAllPaymentTermAsync())
                    .ReturnsAsync(new List<PaymentTermLookupDto>
                    {
                        new() { Id = 1, Description = "Net 30" }
                    });
            }
            if (itemLookup == null)
            {
                itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
                itemLookup.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                            new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            }
            if (hsnLookup == null)
            {
                hsnLookup = new Mock<IHSNLookup>(MockBehavior.Loose);
                hsnLookup.Setup(h => h.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<HSNLookupDto>)ids.Select(id =>
                            new HSNLookupDto { Id = id, HSNCode = "HSN" + id }).ToList());
                hsnLookup.Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<HSNLookupDto> { new() { Id = 1, HSNCode = "HSN1" } });
            }
            if (uomLookup == null)
            {
                uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
                uomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<UOMLookupDto>)ids.Select(id =>
                            new UOMLookupDto { Id = id, Code = "U" + id, UOMName = "UOM " + id }).ToList());
            }
            if (accessFilter == null)
            {
                accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
                accessFilter.Setup(a => a.IsMarketingOfficer()).Returns(false);
            }
            if (ip == null)
            {
                ip = new Mock<IIPAddressService>(MockBehavior.Loose);
                ip.Setup(x => x.GetUserId()).Returns(1);
            }

            return new SalesQuotationQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                partyLookup.Object, paymentTermLookup.Object, itemLookup.Object,
                hsnLookup.Object, uomLookup.Object, accessFilter.Object, ip.Object);
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<(int deliveryTermId, int pendingStatusId)> EnsureMiscChainAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SQQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SQQ_MT", Description = "Quotation",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var dtId = await EnsureMiscAsync(ctx, mt.Id, "SQQ_DT");

            var apr = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (apr == null)
            {
                apr = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "ApprovalStatus",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(apr);
                await ctx.SaveChangesAsync();
            }
            else if (apr.Description != "ApprovalStatus")
            {
                apr.Description = "ApprovalStatus";
                await ctx.SaveChangesAsync();
            }
            var pending = await EnsureMiscAsync(ctx, apr.Id, "Pending");
            return (dtId, pending);
        }

        private async Task<int> SeedAsync(int customerId = 100, int detailCount = 2, IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active, int? statusId = null)
        {
            var (dtId, pending) = await EnsureMiscChainAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var q = new SalesManagement.Domain.Entities.SalesQuotationHeader
            {
                CustomerId = customerId,
                QuotationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ValidityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
                PaymentTermId = 1,
                DeliveryTermId = dtId,
                FreightCharges = 50m, OtherCharges = 20m,
                TotalBasicAmount = 500m, TotalDiscount = 25m,
                NetTaxableAmount = 475m, TotalTax = 23.75m, GrandTotal = 498.75m,
                StatusId = statusId ?? pending,
                Remarks = "test",
                IsActive = active, IsDeleted = deleted,
                SalesQuotationDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.SalesQuotationDetail
                    {
                        ItemId = i * 10, Quantity = i * 5m,
                        ExMillRate = 100m, Discount = 5m,
                        NetRate = 95m, TotalAmount = 475m,
                        HSNId = 1, TaxPercentage = 5m, TaxAmount = 23.75m
                    }).ToList()
            };
            await ctx.SalesQuotationHeader.AddAsync(q);
            await ctx.SaveChangesAsync();
            return q.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync(customerId: 100);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CustomerName_From_Lookup()
        {
            await ClearAsync();
            await SeedAsync(customerId: 100);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows[0].CustomerName.Should().Be("Party 100");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync(deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Details()
        {
            await ClearAsync();
            var id = await SeedAsync(customerId: 5, detailCount: 3);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CustomerId.Should().Be(5);
            result.SalesQuotationDetails.Should().HaveCount(3);
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
            await SeedAsync(customerId: 100);

            var result = await CreateRepo().AutocompleteAsync("", CancellationToken.None);

            result.Should().NotBeEmpty();
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
        public async Task CustomerExistsAsync_Should_Return_True_When_In_Lookup()
        {
            var result = await CreateRepo().CustomerExistsAsync(100);
            result.Should().BeTrue();
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
        public async Task ItemExistsAsync_Should_Return_True()
        {
            var result = await CreateRepo().ItemExistsAsync(10);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HSNExistsAsync_Should_Return_True()
        {
            var result = await CreateRepo().HSNExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeliveryTermExistsAsync_Should_Return_True_For_Active()
        {
            var (dtId, _) = await EnsureMiscChainAsync();

            var result = await CreateRepo().DeliveryTermExistsAsync(dtId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeliveryTermExistsAsync_Should_Return_False_For_Missing()
        {
            var result = await CreateRepo().DeliveryTermExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ContactPersonExistsAsync_Should_Return_False_When_None()
        {
            var result = await CreateRepo().ContactPersonExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesEnquiryExistsAsync_Should_Return_False_When_None()
        {
            var result = await CreateRepo().SalesEnquiryExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsSalesQuotationPendingAsync_Should_Return_True_When_Status_Is_Pending()
        {
            await ClearAsync();
            var id = await SeedAsync();

            var result = await CreateRepo().IsSalesQuotationPendingAsync(id);

            result.Should().BeTrue();
        }

        // ----- Variant / UOM / DiscountType validation methods -----

        [Fact]
        public async Task VariantExistsAsync_Should_Return_True_When_Lookup_Returns_Match()
        {
            var result = await CreateRepo().VariantExistsAsync(42);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task VariantExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var emptyItemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
            emptyItemLookup.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());

            var result = await CreateRepo(itemLookup: emptyItemLookup).VariantExistsAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UOMExistsAsync_Should_Return_True_When_Lookup_Returns_Match()
        {
            var result = await CreateRepo().UOMExistsAsync(7);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UOMExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var emptyUomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
            emptyUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());

            var result = await CreateRepo(uomLookup: emptyUomLookup).UOMExistsAsync(9999);

            result.Should().BeFalse();
        }

        private async Task<int> EnsureQuotDiscTypeAsync(string miscCode = "BY_PERCENTAGE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "QUOT_DISC_TYPE");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "QUOT_DISC_TYPE", Description = "Quotation Discount Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return await EnsureMiscAsync(ctx, mt.Id, miscCode);
        }

        [Fact]
        public async Task DiscountTypeExistsAsync_Should_Return_True_For_QuotDiscType_Row()
        {
            var dtId = await EnsureQuotDiscTypeAsync("BY_PERCENTAGE");

            var result = await CreateRepo().DiscountTypeExistsAsync(dtId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DiscountTypeExistsAsync_Should_Return_False_For_OtherMiscType_Row()
        {
            // dtId belongs to MiscType "SQQ_MT", not QUOT_DISC_TYPE — must be rejected by the filter
            var (dtId, _) = await EnsureMiscChainAsync();

            var result = await CreateRepo().DiscountTypeExistsAsync(dtId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DiscountTypeExistsAsync_Should_Return_False_For_Missing()
        {
            var result = await CreateRepo().DiscountTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
