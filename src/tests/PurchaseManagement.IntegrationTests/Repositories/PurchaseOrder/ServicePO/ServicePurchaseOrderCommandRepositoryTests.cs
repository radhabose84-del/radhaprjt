using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.ServicePO
{
    /// <summary>
    /// Integration tests for ServicePurchaseOrderCommandRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Service POs extend the PO aggregate with service-specific entities:
    /// - ServicePOHeader (service categories, work descriptions)
    /// - ServicePODetail (service line items, schedules)
    /// - ServiceEntrySheet (SES) for service receipt and acceptance
    ///
    /// Constructor requires: ApplicationDbContext, IMiscMasterQueryRepository, IIPAddressService, IDbConnection
    ///
    /// Full testing requires seeding: MiscMaster (ApprovalStatus, ServiceCategory),
    /// ServiceMaster, Vendor (cross-module), Unit (cross-module).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ServicePurchaseOrderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ServicePurchaseOrderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ServicePurchaseOrderCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = 1, Code = "Pending", Description = "Pending",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                });
            var conn = new SqlConnection(_fixture.ConnectionString);
            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            return new ServicePurchaseOrderCommandRepository(ctx, miscMock.Object, _fixture.IpMock.Object, conn, docSeq.Object);
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
    }
}
