using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.PurchaseOrder.Local;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.Local
{
    /// <summary>
    /// Integration tests for PurchaseOrderCommandRepository (Local PO).
    ///
    /// COMPLEXITY NOTE:
    /// Local Purchase Orders involve a deep aggregate:
    /// - PurchaseOrderHeader (aggregate root) contains:
    ///   - List of PurchaseLocalHeader (one per vendor/unit combination)
    ///     - List of PurchaseLocalDetail (line items with Item, UOM, HSN FKs)
    ///   - List of PurchasePaymentTerm
    ///   - List of PurchaseDocument
    /// - StatusId is fetched from MiscMaster (ApprovalStatus -> Pending) during create
    /// - PONumber is auto-generated based on unit + sequence
    /// - Cross-module FKs: VendorId (Party), ItemId (Inventory), CurrencyId (User), HSNId (Inventory)
    ///
    /// Full CRUD testing requires seeding MiscMaster (ApprovalStatus), Items, Vendors, etc.
    /// Basic instantiation and minimal tests are provided here.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PurchaseOrderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseOrderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private static Mock<IMiscMasterQueryRepository> BuildMiscMock(int pendingStatusId = 100)
        {
            var mock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            mock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = pendingStatusId,
                    Code = "PENDING",
                    Description = "Pending",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return mock;
        }

        private PurchaseOrderCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IMiscMasterQueryRepository>? miscMock = null)
        {
            miscMock ??= BuildMiscMock();
            return new PurchaseOrderCommandRepository(ctx, miscMock.Object, _fixture.IpMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateWithoutTransactionAsync_Should_Persist_Header()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            // Seed real MiscMaster for the Pending status lookup
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "ApprovalStatus",
                Description = "Approval Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var pendingMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "Pending",
                Description = "Pending",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(pendingMisc);
            await ctx.SaveChangesAsync();

            // Seed POCategory MiscMaster (FK required on PurchaseOrderHeader)
            var poCatType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "POCategory")
                ?? new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "POCategory", Description = "PO Category",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
            if (poCatType.Id == 0) { ctx.MiscTypeMaster.Add(poCatType); await ctx.SaveChangesAsync(); }

            var poCatMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = poCatType.Id, Code = "LOCAL", Description = "Local PO", SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(poCatMisc);
            await ctx.SaveChangesAsync();

            var miscMock = BuildMiscMock(pendingMisc.Id);

            var aggregate = new PurchaseOrderHeader
            {
                PONumber = "PO-TEST-001",
                PODate = DateTimeOffset.UtcNow,
                UnitId = 1,
                VendorId = 100,
                CurrencyId = 1,
                POCategoryId = poCatMisc.Id,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var repo = CreateRepo(ctx, miscMock);
            var id = await repo.CreateWithoutTransactionAsync(aggregate, CancellationToken.None);
            await ctx.SaveChangesAsync();

            id.Should().BeGreaterThan(0);
            aggregate.StatusId.Should().Be(pendingMisc.Id);
        }
    }
}
