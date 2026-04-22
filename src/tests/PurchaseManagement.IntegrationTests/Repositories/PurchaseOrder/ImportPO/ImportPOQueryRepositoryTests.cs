using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ImportPO;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.ImportPO
{
    /// <summary>
    /// Integration tests for ImportPOQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Import PO queries use complex Dapper SQL joining:
    /// - Purchase.PurchaseOrderHeader, Purchase.ImportPOHeader, Purchase.ImportPODetail
    /// - Purchase.MiscMaster (status, PO method), Purchase.PortMaster
    /// - Cross-module: Vendor, Item, Currency, UOM, HSN
    ///
    /// Constructor requires: IDbConnection, IIPAddressService, IMiscMasterQueryRepository
    ///
    /// GetByIdAsync returns a full ImportPOFullVm with header summary, import headers,
    /// import details, payment terms, and documents.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ImportPOQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ImportPOQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ImportPOQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            return new ImportPOQueryRepository(conn, _fixture.IpMock.Object, miscMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
