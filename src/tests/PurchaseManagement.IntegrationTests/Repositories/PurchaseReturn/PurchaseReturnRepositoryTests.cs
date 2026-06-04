using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseReturn;

[Collection("DatabaseCollection")]
public sealed class PurchaseReturnRepositoryTests
{
    private readonly DbFixture _fixture;

    public PurchaseReturnRepositoryTests(DbFixture fixture) => _fixture = fixture;

    private PurchaseReturnCommandRepository CreateCommandRepo()
    {
        var ctx = _fixture.CreateFreshDbContext();
        var docSeqLookup = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose).Object;
        return new PurchaseReturnCommandRepository(ctx, docSeqLookup);
    }

    private PurchaseReturnQueryRepository CreateQueryRepo()
    {
        var conn = new SqlConnection(_fixture.ConnectionString);
        var partyLookup = new Mock<IPartyLookup>(MockBehavior.Loose).Object;
        var itemLookup = new Mock<IItemLookup>(MockBehavior.Loose).Object;
        var uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose).Object;
        return new PurchaseReturnQueryRepository(conn, partyLookup, itemLookup, uomLookup);
    }

    [Fact]
    public void CommandRepository_Should_Be_Instantiable()
    {
        var repo = CreateCommandRepo();
        repo.Should().NotBeNull();
    }

    [Fact]
    public void QueryRepository_Should_Be_Instantiable()
    {
        var repo = CreateQueryRepo();
        repo.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Empty_When_NoData()
    {
        await _fixture.ClearAllTablesAsync();

        var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetReturnableQtyByGrnAsync_Should_Return_Empty_When_NoData()
    {
        await _fixture.ClearAllTablesAsync();
        var result = await CreateQueryRepo().GetReturnableQtyByGrnAsync(9999, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task NotFoundAsync_Returns_True_For_NonExistentId()
    {
        await _fixture.ClearAllTablesAsync();
        var notFound = await CreateQueryRepo().NotFoundAsync(9999);
        notFound.Should().BeTrue();
    }

    [Fact]
    public async Task GetPosByVendorAsync_Should_Return_Empty_When_NoData()
    {
        await _fixture.ClearAllTablesAsync();
        var result = await CreateQueryRepo().GetPosByVendorAsync(9999, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGrnsByVendorPoAsync_Should_Return_Empty_When_NoData()
    {
        await _fixture.ClearAllTablesAsync();
        var result = await CreateQueryRepo().GetGrnsByVendorPoAsync(9999, 9999, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingAsync_Should_Return_Empty_When_NoData()
    {
        await _fixture.ClearAllTablesAsync();
        var result = await CreateQueryRepo().GetPendingAsync(1, 20, null, CancellationToken.None);
        result.Should().BeEmpty();
    }
}
