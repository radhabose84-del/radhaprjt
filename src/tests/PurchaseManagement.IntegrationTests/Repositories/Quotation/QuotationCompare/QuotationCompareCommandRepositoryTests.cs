using Microsoft.EntityFrameworkCore;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationCompare;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.QuotationCompare
{
    /// <summary>
    /// Integration tests for QuotationCompareCommandRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Quotation Comparison aggregates quotations from multiple suppliers for an RFQ:
    /// - QuotationComparisonHeader links to RfqId, contains StatusId (approval flow)
    /// - QuotationComparisonDetail has per-supplier pricing comparisons
    /// - StatusId is fetched from MiscMaster (ApprovalStatus -> Pending) during creation
    /// - Creates/updates PriceMaster entries when comparison is approved
    ///
    /// Constructor requires: ApplicationDbContext, IIPAddressService, IMiscMasterQueryRepository
    ///
    /// Full testing requires: RfqMaster, QuotationHeader (multiple), MiscMaster chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class QuotationCompareCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QuotationCompareCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QuotationCompareCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            return new QuotationCompareCommandRepository(ctx, _fixture.IpMock.Object, miscMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);
            repo.Should().NotBeNull();
        }

        // ---------------------------------------------------------------
        // Quotation Comparison create flow: AddAsync then
        // GetByIdQuoteComparisonWorkFlowAsync must both succeed end to end.
        // ---------------------------------------------------------------

        private async Task<(int rfqId, string rfqCode, int quotationHeaderId, int quotationDetailId)>
            SeedComparisonGraphAsync(ApplicationDbContext ctx)
        {
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "ApprovalStatus",
                Description = "ApprovalStatus"
            };
            ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().Add(miscType);
            await ctx.SaveChangesAsync();

            var pending = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "Pending",
                Description = "Pending",
                SortOrder = 0
            };
            ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().Add(pending);
            await ctx.SaveChangesAsync();

            var rfq = new RfqMaster
            {
                UnitId = 1,
                RfqCode = "RFQ-CMP-1587",
                RfqStatusId = pending.Id
            };
            // LastSubmitDate has a private setter — set via reflection.
            typeof(RfqMaster).GetProperty(nameof(RfqMaster.LastSubmitDate))!
                .SetValue(rfq, DateOnly.FromDateTime(DateTime.Now));
            ctx.Set<RfqMaster>().Add(rfq);
            await ctx.SaveChangesAsync();

            var qHeader = new QuotationHeader
            {
                UnitId = 1,
                RfqId = rfq.Id,
                SupplierId = 1,
                QuotationNumber = "Q-CMP-1587",
                ValidTill = DateOnly.FromDateTime(DateTime.Now),
                TaxableSubtotal = 490,
                GstTotal = 24.5m,
                ItemsTotal = 514.5m,
                GrandTotal = 514.5m
            };
            ctx.Set<QuotationHeader>().Add(qHeader);
            await ctx.SaveChangesAsync();

            var qDetail = new QuotationDetail
            {
                QuotationHeaderId = qHeader.Id,
                ItemId = 157,
                HsnId = 0,
                UomId = 3,
                CurrencyId = 6,
                Quantity = 5,
                Rate = 100,
                Discount = 10,
                GstPercent = 5,
                LineSubtotal = 490,
                GstAmount = 24.5m,
                Total = 514.5m
            };
            ctx.Set<QuotationDetail>().Add(qDetail);
            await ctx.SaveChangesAsync();

            return (rfq.Id, rfq.RfqCode, qHeader.Id, qDetail.Id);
        }

        [Fact]
        public async Task AddAsync_Then_GetByIdWorkFlow_Should_Both_Succeed()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (rfqId, rfqCode, qhId, qdId) = await SeedComparisonGraphAsync(ctx);

            // Mirrors exactly what AutoMapper produces for the handler.
            var header = new QuotationComparisonHeader
            {
                RfqId = rfqId,
                RfqCode = rfqCode,
                QuotationConfirmedDetails = new List<QuotationComparisonDetail>
                {
                    new()
                    {
                        QuotationHeaderId = qhId,
                        QuotationDetailId = qdId,
                        Net = 90m,
                        LandedUnit = 114.5m,
                        Total = 572.5m,
                        OverrideStatus = false,
                        Remarks = ""
                    }
                }
            };

            var repo = CreateRepo(ctx);

            var newId = await repo.AddAsync(header);
            newId.Should().BeGreaterThan(0);

            var workflow = await repo.GetByIdQuoteComparisonWorkFlowAsync(newId);

            workflow.Should().NotBeNull();
            workflow.Id.Should().Be(newId);
            workflow.RfqId.Should().Be(rfqId);
            workflow.RfqCode.Should().Be(rfqCode);
            workflow.UnitId.Should().Be(1); // DbFixture IP mock GetUnitId() = 1
        }

        // Submitting a comparison twice for the same RFQ updates the existing
        // one (replaces detail lines, keeps the same header id) instead of
        // inserting a duplicate.
        [Fact]
        public async Task AddAsync_SameRfq_Twice_Upserts_NoDuplicate()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (rfqId, rfqCode, qhId, qdId) = await SeedComparisonGraphAsync(ctx);
            var repo = CreateRepo(ctx);

            // First submission (create).
            var firstId = await repo.AddAsync(new QuotationComparisonHeader
            {
                RfqId = rfqId,
                RfqCode = rfqCode,
                QuotationConfirmedDetails = new List<QuotationComparisonDetail>
                {
                    new()
                    {
                        QuotationHeaderId = qhId,
                        QuotationDetailId = qdId,
                        Net = 90m,
                        LandedUnit = 114.5m,
                        Total = 572.5m,
                        OverrideStatus = false,
                        Remarks = "first"
                    }
                }
            });
            firstId.Should().BeGreaterThan(0);

            // Second submission for the SAME RFQ (the "Confirm" action) with
            // different values — must update, not duplicate or error.
            var secondId = await repo.AddAsync(new QuotationComparisonHeader
            {
                RfqId = rfqId,
                RfqCode = rfqCode,
                QuotationConfirmedDetails = new List<QuotationComparisonDetail>
                {
                    new()
                    {
                        QuotationHeaderId = qhId,
                        QuotationDetailId = qdId,
                        Net = 75m,
                        LandedUnit = 99.9m,
                        Total = 499.5m,
                        OverrideStatus = false,
                        Remarks = "second"
                    }
                }
            });

            secondId.Should().Be(firstId); // same header, updated in place

            await using var verify = _fixture.CreateFreshDbContext();

            var headerCount = verify.Set<QuotationComparisonHeader>()
                .Count(h => h.RfqId == rfqId);
            headerCount.Should().Be(1); // no duplicate header

            var saved = verify.Set<QuotationComparisonHeader>()
                .Include(h => h.QuotationConfirmedDetails)
                .First(h => h.RfqId == rfqId);

            saved.QuotationConfirmedDetails.Should().HaveCount(1);
            saved.QuotationConfirmedDetails.First().Net.Should().Be(75m); // replaced with 2nd submission
            saved.QuotationConfirmedDetails.First().Remarks.Should().Be("second");
        }
    }
}
