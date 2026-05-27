using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseReturn;

/// <summary>
/// Integration tests for the ReturnType repository pair.
/// Smoke-level: confirms the repos instantiate and that the empty-state queries return clean results.
/// </summary>
[Collection("DatabaseCollection")]
public sealed class ReturnTypeRepositoryTests
{
    private readonly DbFixture _fixture;

    public ReturnTypeRepositoryTests(DbFixture fixture) => _fixture = fixture;

    private ReturnTypeCommandRepository CreateCommandRepo()
    {
        var ctx = _fixture.CreateFreshDbContext();
        return new ReturnTypeCommandRepository(ctx, _fixture.IpMock.Object);
    }

    private ReturnTypeQueryRepository CreateQueryRepo()
    {
        var conn = new SqlConnection(_fixture.ConnectionString);
        return new ReturnTypeQueryRepository(conn);
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
    public async Task NotFoundAsync_Returns_True_For_NonExistentId()
    {
        await _fixture.ClearAllTablesAsync();
        var notFound = await CreateQueryRepo().NotFoundAsync(9999);
        notFound.Should().BeTrue();
    }
}
