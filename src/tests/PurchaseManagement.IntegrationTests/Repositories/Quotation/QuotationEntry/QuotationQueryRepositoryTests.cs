using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationEntry;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.QuotationEntry
{
    /// <summary>
    /// Integration tests for QuotationQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Quotation queries use EF Core with cross-module lookups:
    /// - IItemLookup, IUOMLookup, IPartyLookup, ICurrencyLookup, ICompanyLookup, IUnitLookup
    ///
    /// Primary constructor: (ApplicationDbContext db, IItemLookup, IUOMLookup, IPartyLookup,
    ///   IIPAddressService, ICurrencyLookup, ICompanyLookup, IUnitLookup)
    ///
    /// GetAllAsync returns paginated quotation list.
    /// GetByIdAsync returns full quotation with lines and lookup-populated names.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class QuotationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QuotationQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QuotationQueryRepository CreateRepo()
        {
            var ctx = _fixture.CreateFreshDbContext();
            return new QuotationQueryRepository(
                ctx,
                new Mock<IItemLookup>(MockBehavior.Loose).Object,
                new Mock<IUOMLookup>(MockBehavior.Loose).Object,
                new Mock<IPartyLookup>(MockBehavior.Loose).Object,
                _fixture.IpMock.Object,
                new Mock<ICurrencyLookup>(MockBehavior.Loose).Object,
                new Mock<ICompanyLookup>(MockBehavior.Loose).Object,
                new Mock<IUnitLookup>(MockBehavior.Loose).Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var (items, total) = await CreateRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }
    }
}
