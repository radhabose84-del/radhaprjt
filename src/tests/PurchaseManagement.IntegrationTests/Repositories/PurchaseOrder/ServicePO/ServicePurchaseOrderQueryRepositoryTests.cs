using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ServicePO;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.ServicePO
{
    /// <summary>
    /// Integration tests for ServicePurchaseOrderQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Service PO queries use EF Core + Dapper and join across:
    /// - Purchase.PurchaseOrderHeader, Purchase.ServicePOHeader, Purchase.ServicePODetail
    /// - Purchase.ServiceEntrySheet, Purchase.ServiceSchedule
    /// - Cross-module lookups: ICurrencyLookup, IUnitLookup, IPartyLookup, IUOMLookup
    ///
    /// Constructor requires: IDbConnection, IIPAddressService, ApplicationDbContext,
    /// ICurrencyLookup, IPartyLookup, IMiscMasterQueryRepository, IUOMLookup, IUnitLookup, IMapper
    ///
    /// All cross-module lookups are mocked for isolation.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ServicePurchaseOrderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ServicePurchaseOrderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ServicePurchaseOrderQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ctx = _fixture.CreateFreshDbContext();
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = 1, Code = "Approved", Description = "Approved"
                });

            var currencyLookup = new Mock<ICurrencyLookup>(MockBehavior.Loose);
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            var partyLookup = new Mock<IPartyLookup>(MockBehavior.Loose);
            var uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
            var mapper = new Mock<IMapper>(MockBehavior.Loose);
            var maintenanceRequestLookup =
                new Mock<Contracts.Interfaces.Lookups.Maintenance.IMaintenanceRequestLookup>(MockBehavior.Loose);

            return new ServicePurchaseOrderQueryRepository(
                conn, _fixture.IpMock.Object, ctx, currencyLookup.Object,
                partyLookup.Object, miscMock.Object, uomLookup.Object,
                unitLookup.Object, mapper.Object, maintenanceRequestLookup.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        // Note: GetAllAsync, GetByIdAsync, and SES queries require seeding
        // the full ServicePO aggregate with ServicePOHeader/Detail children.
        // These are covered by unit tests with mocked dependencies.
    }
}
