using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
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
    }
}
