using Microsoft.Data.SqlClient;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class OcrQualityParameterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OcrQualityParameterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private OcrQualityParameterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        // Seeds the full chain ArrivalHeader → RawMaterialPOHeader → OCREntry and returns
        // (arrivalHeaderId, ocrId, qualityTemplateId). EF inserts handle NOT NULL + audit fields.
        private async Task<(int arrivalId, int ocrId, int templateId)> SeedChainAsync(
            int qualityTemplateId = 555, string arrivalNumber = "ARV-OQP-0001", string ocrNumber = "OCR-OQP-0001", string poNumber = "PO-OQP-0001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Reference masters required by OCREntry / RawMaterialPOHeader FK constraints.
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = $"MT-{ocrNumber}", Description = "Test Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new PurchaseManagement.Domain.Entities.MiscMaster
            { Code = $"C-{ocrNumber}", Description = "Misc", MiscTypeId = miscType.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().Add(misc);
            await ctx.SaveChangesAsync();

            var paymentTerm = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            { Code = $"PT-{ocrNumber}", Description = "30 Days", BaselineTypeId = misc.Id, CreditDays = 30, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<PurchaseManagement.Domain.Entities.PaymentTermMaster>().Add(paymentTerm);
            await ctx.SaveChangesAsync();

            var ocr = new PurchaseManagement.Domain.Entities.OCREntry
            {
                OcrNumber = ocrNumber, OcrDate = DateTimeOffset.UtcNow,
                ProcurementSourceId = misc.Id, ProcurementTypeId = misc.Id, BrokerDirectId = misc.Id,
                StatusId = misc.Id, PaymentTermId = paymentTerm.Id, QualityTemplateId = qualityTemplateId,
                SupplierId = 10, LocationId = 11, StationId = 12, ItemId = 13, CountId = 14,
                Quantity = 800m, Rate = 68500m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseManagement.Domain.Entities.OCREntry>().Add(ocr);
            await ctx.SaveChangesAsync();

            var rmpo = new RawMaterialPOHeader
            {
                UnitId = 1, PONumber = poNumber, PODate = DateTimeOffset.UtcNow,
                OcrId = ocr.Id, ProcurementDocumentTypeId = misc.Id, StatusId = misc.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<RawMaterialPOHeader>().Add(rmpo);
            await ctx.SaveChangesAsync();

            var arrival = new ArrivalHeader
            {
                UnitId = 1, ArrivalNumber = arrivalNumber, ArrivalDate = DateTimeOffset.UtcNow,
                RawMaterialPOId = rmpo.Id, VehicleNumber = "TN-38-BC-4521",
                SupplierId = 10, StationId = 12, GodownId = 5, TransporterId = 7,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<ArrivalHeader>().Add(arrival);
            await ctx.SaveChangesAsync();

            return (arrival.Id, ocr.Id, qualityTemplateId);
        }

        private async Task AddParamAsync(int ocrId, int templateId, int paramId, string value,
            IsDelete isDeleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.Set<PurchaseManagement.Domain.Entities.OCRQualityParameter>().Add(
                new PurchaseManagement.Domain.Entities.OCRQualityParameter
                {
                    OcrId = ocrId, QualityTemplateId = templateId, ParamId = paramId, Value = value,
                    IsActive = Status.Active, IsDeleted = isDeleted
                });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByArrivalHeaderIdAsync_Should_Return_Params_For_The_Ocr()
        {
            await _fixture.ClearAllTablesAsync();
            var (arrivalId, ocrId, templateId) = await SeedChainAsync();
            await AddParamAsync(ocrId, templateId, paramId: 101, value: "29.50+ MM");
            await AddParamAsync(ocrId, templateId, paramId: 102, value: "3.70 - 4.30");

            var result = await CreateRepo().GetByArrivalHeaderIdAsync(arrivalId);

            result.Should().HaveCount(2);
            result.Select(r => r.ParamId).Should().BeEquivalentTo(new[] { 101, 102 });
            result.Single(r => r.ParamId == 101).Value.Should().Be("29.50+ MM");
        }

        [Fact]
        public async Task GetByArrivalHeaderIdAsync_Should_Exclude_SoftDeleted_Params()
        {
            await _fixture.ClearAllTablesAsync();
            var (arrivalId, ocrId, templateId) = await SeedChainAsync();
            await AddParamAsync(ocrId, templateId, paramId: 101, value: "Keep");
            await AddParamAsync(ocrId, templateId, paramId: 102, value: "Gone", isDeleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByArrivalHeaderIdAsync(arrivalId);

            result.Should().ContainSingle();
            result.Single().ParamId.Should().Be(101);
        }

        [Fact]
        public async Task GetByArrivalHeaderIdAsync_Should_Scope_To_The_Exact_Ocr()
        {
            await _fixture.ClearAllTablesAsync();
            // Two chains sharing the same QualityTemplateId — scoping must use OcrId, not just template.
            var first = await SeedChainAsync(qualityTemplateId: 777, arrivalNumber: "ARV-OQP-A", ocrNumber: "OCR-OQP-A", poNumber: "PO-OQP-A");
            var second = await SeedChainAsync(qualityTemplateId: 777, arrivalNumber: "ARV-OQP-B", ocrNumber: "OCR-OQP-B", poNumber: "PO-OQP-B");
            await AddParamAsync(first.ocrId, 777, paramId: 201, value: "First-Only");
            await AddParamAsync(second.ocrId, 777, paramId: 202, value: "Second-Only");

            var result = await CreateRepo().GetByArrivalHeaderIdAsync(first.arrivalId);

            result.Should().ContainSingle();
            result.Single().ParamId.Should().Be(201);
        }

        [Fact]
        public async Task GetByArrivalHeaderIdAsync_Should_Return_Empty_When_No_Params()
        {
            await _fixture.ClearAllTablesAsync();
            var (arrivalId, _, _) = await SeedChainAsync();

            var result = await CreateRepo().GetByArrivalHeaderIdAsync(arrivalId);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByArrivalHeaderIdAsync_Should_Return_Empty_For_Unknown_Arrival()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByArrivalHeaderIdAsync(999999);

            result.Should().BeEmpty();
        }
    }
}
