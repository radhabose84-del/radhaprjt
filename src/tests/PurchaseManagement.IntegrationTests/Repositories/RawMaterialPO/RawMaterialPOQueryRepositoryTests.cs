using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.RawMaterialPO;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.RawMaterialPO
{
    [Collection("DatabaseCollection")]
    public sealed class RawMaterialPOQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _docSeqMock = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _itemMock = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _hsnMock = new(MockBehavior.Loose);

        public RawMaterialPOQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _docSeqMock
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Returns(Task.CompletedTask);
            _itemMock
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 13, ItemName = "Cotton" } });
            _hsnMock
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto> { new() { Id = 1, HSNCode = "5201" } });
        }

        private RawMaterialPOQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new RawMaterialPOQueryRepository(conn, _itemMock.Object, _hsnMock.Object);
        }

        private RawMaterialPOCommandRepository CreateCommandRepo(ApplicationDbContext ctx) =>
            new(ctx, _docSeqMock.Object);

        private static RawMaterialPOHeader BuildHeader(int ocrId, int docTypeId, int statusId, string poNumber, decimal qty = 500m) =>
            new()
            {
                UnitId = 1, PONumber = poNumber, PODate = DateTimeOffset.UtcNow,
                OcrId = ocrId, ProcurementDocumentTypeId = docTypeId, StatusId = statusId,
                TaxableTotal = qty * 68500m, TotalGstAmount = 0m, NetTotal = qty * 68500m,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                RawMaterialPODetails = new List<RawMaterialPODetail>
                {
                    new() { ItemId = 13, HsnId = 1, Quantity = qty, Rate = 68500m, ItemValue = qty * 68500m, TotalGST = 0m, NetValue = qty * 68500m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
                }
            };

        private async Task<(int ocrId, int docTypeId, int statusId)> SeedAsync(ApplicationDbContext ctx, decimal ocrQty = 800m)
        {
            await _fixture.ClearAllTablesAsync();

            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscTypeMaster
                { MiscTypeCode = "MT001", Description = "Test Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var approved = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "APPR", Description = "Approved", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var docType = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "PO", Description = "Purchase Order", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var convStatus = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.MiscMaster
                { Code = "PART", Description = "Partially Converted", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var paymentTerm = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            { Code = "PT001", Description = "30 Days", BaselineTypeId = approved.Id, CreditDays = 30, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<PurchaseManagement.Domain.Entities.PaymentTermMaster>().Add(paymentTerm);
            await ctx.SaveChangesAsync();

            var ocr = new PurchaseManagement.Domain.Entities.OCREntry
            {
                OcrNumber = "OCR-2025-0004", OcrDate = DateTimeOffset.UtcNow,
                ProcurementSourceId = docType.Id, ProcurementTypeId = docType.Id, BrokerDirectId = docType.Id,
                StatusId = approved.Id, PaymentTermId = paymentTerm.Id,
                SupplierId = 10, LocationId = 11, StationId = 12, ItemId = 13, CountId = 14,
                Quantity = ocrQty, Rate = 68500m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().Add(ocr);
            await ctx.SaveChangesAsync();

            return (ocr.Id, docType.Id, convStatus.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, docTypeId, statusId) = await SeedAsync(ctx);
            var id = await CreateCommandRepo(ctx).CreateAsync(BuildHeader(ocrId, docTypeId, statusId, "RMPO-Q-0001"), 0, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.PONumber.Should().Be("RMPO-Q-0001");
            dto.OcrNumber.Should().Be("OCR-2025-0004");
            dto.Details.Should().HaveCount(1);
            dto.Details[0].ItemName.Should().Be("Cotton");
            dto.Details[0].HsnCode.Should().Be("5201");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, docTypeId, statusId) = await SeedAsync(ctx);
            await CreateCommandRepo(ctx).CreateAsync(BuildHeader(ocrId, docTypeId, statusId, "RMPO-Q-0002"), 0, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, docTypeId, statusId) = await SeedAsync(ctx);
            var id = await CreateCommandRepo(ctx).CreateAsync(BuildHeader(ocrId, docTypeId, statusId, "RMPO-Q-0003"), 0, CancellationToken.None);
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetConvertedQuantityAsync_Should_Sum_Detail_Quantities()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, docTypeId, statusId) = await SeedAsync(ctx);
            await CreateCommandRepo(ctx).CreateAsync(BuildHeader(ocrId, docTypeId, statusId, "RMPO-Q-0004", qty: 500m), 0, CancellationToken.None);

            var converted = await CreateQueryRepo().GetConvertedQuantityAsync(ocrId, null);

            converted.Should().Be(500m);
        }

        [Fact]
        public async Task OcrExistsAndApprovedAsync_Should_Return_True_For_Approved_Ocr()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (ocrId, _, _) = await SeedAsync(ctx);

            var approved = await CreateQueryRepo().OcrExistsAndApprovedAsync(ocrId);

            approved.Should().BeTrue();
        }
    }
}
