using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BlanketPO;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.BlanketPO
{
    [Collection("DatabaseCollection")]
    public sealed class BlanketPOQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public BlanketPOQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // The misc/ip dependencies are not exercised by the SQL-only methods tested below.
        private BlanketPOQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString),
                new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose).Object,
                new Mock<IIPAddressService>(MockBehavior.Loose).Object);

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().NotFoundAsync(9999999, CancellationToken.None);

            result.Should().BeTrue();
        }
    }
}
