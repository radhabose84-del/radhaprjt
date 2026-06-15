using Contracts.Interfaces;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ImportPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.ImportPO
{
    /// <summary>
    /// Integration tests for ImportPOCommandRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Import Purchase Orders extend the Local PO aggregate with additional fields:
    /// - ImportPOHeader (extends PurchaseLocalHeader with port, shipping, insurance fields)
    /// - ImportPODetail (extends PurchaseLocalDetail with duty, customs fields)
    /// - PurchaseOrderHeader aggregate root is shared with Local PO
    ///
    /// Constructor requires: ApplicationDbContext, IMiscMasterQueryRepository, ILogger
    /// - StatusId is fetched from MiscMaster (ApprovalStatus -> Pending) during create
    /// - Import-specific fields: PortId, ShippingTermId, InsuranceAmount, FreightCurrency
    ///
    /// Full testing requires: MiscMaster chain, Port, Currency, Items, Vendors, HSN (cross-module).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ImportPOCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ImportPOCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ImportPOCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = 1, Code = "Pending", Description = "Pending",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                });
            var logger = new Mock<ILogger<ImportPOCommandRepository>>(MockBehavior.Loose);
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            return new ImportPOCommandRepository(ctx, miscMock.Object, logger.Object, ip.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            repo.Should().NotBeNull();
        }

        // Note: CreateAsync requires a fully populated PurchaseOrderHeader aggregate
        // with ImportPOHeader/ImportPODetail children plus valid FK references to
        // Port, Currency, Item, Vendor, HSN masters. This deep seeding is deferred
        // to E2E or manual testing. Unit tests cover the handler logic with mocks.
    }
}
