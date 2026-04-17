using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.Local;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.Local
{
    /// <summary>
    /// Integration tests for PurchaseOrderQueryRepository (Local PO).
    ///
    /// COMPLEXITY NOTE:
    /// PO query methods involve multi-table SQL JOINs across:
    /// - Purchase.PurchaseOrderHeader, Purchase.PurchaseLocalHeader, Purchase.PurchaseLocalDetail
    /// - Purchase.MiscMaster (status, PO method, approval status)
    /// - Cross-module lookups: Vendor (Party), Item (Inventory), Currency (User)
    ///
    /// Constructor requires: IDbConnection, IIPAddressService, ApplicationDbContext, IMiscMasterQueryRepository
    ///
    /// GetAllAsync is testable with seeded PO data.
    /// Complex queries (GetPOLocalPending, approved indent details) require full PO + indent chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PurchaseOrderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseOrderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PurchaseOrderQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = 1, Code = "Approved", Description = "Approved"
                });
            var ctx = _fixture.CreateFreshDbContext();
            return new PurchaseOrderQueryRepository(conn, _fixture.IpMock.Object, ctx, miscMock.Object);
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

            var result = await CreateRepo().GetAllAsync(1, 10, null, null, null, null, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }
    }
}
