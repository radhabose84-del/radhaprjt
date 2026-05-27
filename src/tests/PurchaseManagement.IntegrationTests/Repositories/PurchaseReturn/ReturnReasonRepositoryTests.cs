using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseReturn;

[Collection("DatabaseCollection")]
public sealed class ReturnReasonRepositoryTests
{
    private readonly DbFixture _fixture;

    public ReturnReasonRepositoryTests(DbFixture fixture) => _fixture = fixture;

    private ReturnReasonCommandRepository CreateCommandRepo()
    {
        var ctx = _fixture.CreateFreshDbContext();
        return new ReturnReasonCommandRepository(ctx, _fixture.IpMock.Object);
    }

    private ReturnReasonQueryRepository CreateQueryRepo()
    {
        var conn = new SqlConnection(_fixture.ConnectionString);
        return new ReturnReasonQueryRepository(conn);
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
    public async Task GetByReturnTypeIdAsync_Should_Return_Empty_When_NoData()
    {
        await _fixture.ClearAllTablesAsync();
        var result = await CreateQueryRepo().GetByReturnTypeIdAsync(9999, CancellationToken.None);
        result.Should().BeEmpty();
    }
}
