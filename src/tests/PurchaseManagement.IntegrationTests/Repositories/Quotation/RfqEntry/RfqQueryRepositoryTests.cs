using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.RfqEntry
{
    /// <summary>
    /// Integration tests for RfqQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// RFQ queries use EF Core with eager loading:
    /// - RfqMaster.Include(Items).Include(Suppliers).Include(RfqStatus).Include(InitiationType)
    /// - GetAllAsync uses pagination with unit-scoped filtering
    ///
    /// Constructor requires: ApplicationDbContext, IIPAddressService, IMiscMasterQueryRepository
    ///
    /// GetAggregateAsync returns full RFQ with items and suppliers (EF Core).
    /// GetAllAsync returns paginated list with status filtering.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class RfqQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RfqQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RfqQueryRepository CreateRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            return new RfqQueryRepository(ctx, _fixture.IpMock.Object, miscMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAggregateAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetAggregateAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (items, total) = await CreateRepo(ctx).GetAllAsync(1, 10, null, null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ---------------------------------------------------------------
        // Pending comparison list: an RFQ is "pending for comparison" when
        // its submission deadline has reached or passed as of the current
        // date. Deadlines today or in the past are included; future
        // deadlines are excluded.
        // ---------------------------------------------------------------

        private RfqQueryRepository CreateRepoWithPending(ApplicationDbContext ctx, int pendingId)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock
                .Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = pendingId,
                    Code = MiscEnumEntity.Pending
                });
            return new RfqQueryRepository(ctx, _fixture.IpMock.Object, miscMock.Object);
        }

        private async Task<int> SeedRfqWithQuotationAsync(
            ApplicationDbContext ctx, string rfqCode, DateOnly lastSubmitDate)
        {
            // Status row (also satisfies the RfqMaster.RfqStatusId FK).
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = MiscEnumEntity.ApprovalStatus,
                Description = MiscEnumEntity.ApprovalStatus
            };
            ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().Add(miscType);
            await ctx.SaveChangesAsync();

            var status = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = MiscEnumEntity.Pending,
                Description = MiscEnumEntity.Pending,
                SortOrder = 0
            };
            ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().Add(status);
            await ctx.SaveChangesAsync();

            var rfq = new RfqMaster
            {
                UnitId = 1,                       // matches DbFixture IP mock GetUnitId() = 1
                RfqCode = rfqCode,
                RfqStatusId = status.Id
            };
            // LastSubmitDate has a private setter — set via reflection.
            typeof(RfqMaster)
                .GetProperty(nameof(RfqMaster.LastSubmitDate))!
                .SetValue(rfq, lastSubmitDate);
            ctx.Set<RfqMaster>().Add(rfq);
            await ctx.SaveChangesAsync();

            ctx.Set<QuotationHeader>().Add(new QuotationHeader
            {
                UnitId = 1,
                RfqId = rfq.Id,
                SupplierId = 1,
                QuotationNumber = $"Q-{rfqCode}",
                ValidTill = lastSubmitDate.AddDays(30)
            });
            await ctx.SaveChangesAsync();

            return status.Id;
        }

        [Fact]
        public async Task GetRfqAutoCompleteComparisonAsync_Pending_Should_Return_Rfq_With_Past_LastSubmitDate()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var past = DateOnly.FromDateTime(DateTime.Now).AddDays(-5);
            var pendingId = await SeedRfqWithQuotationAsync(ctx, "RFQ-PAST-01", past);

            var result = await CreateRepoWithPending(ctx, pendingId)
                .GetRfqAutoCompleteComparisonAsync(null, null, pendingId, CancellationToken.None);

            result.Should().ContainSingle(x => x.RfqCode == "RFQ-PAST-01");
        }

        [Fact]
        public async Task GetRfqAutoCompleteComparisonAsync_Pending_Should_Exclude_Rfq_With_Future_LastSubmitDate()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var future = DateOnly.FromDateTime(DateTime.Now).AddDays(5);
            var pendingId = await SeedRfqWithQuotationAsync(ctx, "RFQ-FUTURE-01", future);

            var result = await CreateRepoWithPending(ctx, pendingId)
                .GetRfqAutoCompleteComparisonAsync(null, null, pendingId, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRfqAutoCompleteComparisonAsync_Pending_Should_Include_Rfq_With_Today_LastSubmitDate()
        {
            // Boundary: a deadline equal to today is included (inclusive).
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var pendingId = await SeedRfqWithQuotationAsync(ctx, "RFQ-TODAY-01", today);

            var result = await CreateRepoWithPending(ctx, pendingId)
                .GetRfqAutoCompleteComparisonAsync(null, null, pendingId, CancellationToken.None);

            result.Should().ContainSingle(x => x.RfqCode == "RFQ-TODAY-01");
        }
    }
}
