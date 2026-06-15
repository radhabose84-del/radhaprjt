using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Updates.Purchase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QCManagement.Domain.Entities;
using QCManagement.Infrastructure.Repositories.QcInspection;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QcInspection
{
    [Collection("DatabaseCollection")]
    public sealed class QcInspectionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QcInspectionQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QcInspectionQueryRepository CreateRepo() =>
            CreateRepo(new Mock<IOcrQualityParameterLookup>(MockBehavior.Loose).Object);

        private QcInspectionQueryRepository CreateRepo(IOcrQualityParameterLookup ocrLookup) =>
            new(
                new SqlConnection(_fixture.ConnectionString),
                new Mock<IGrnLookup>(MockBehavior.Loose).Object,
                new Mock<IArrivalLookup>(MockBehavior.Loose).Object,
                new Mock<IItemLookup>(MockBehavior.Loose).Object,
                new Mock<ISupplierLookup>(MockBehavior.Loose).Object,
                new Mock<IInventoryCategoryLookup>(MockBehavior.Loose).Object,
                ocrLookup);

        // SourceTypeId is FK to QC.MiscMaster â€” get-or-create a source-type row and return its Id.
        // MiscMaster/MiscTypeMaster are shared reference tables (used by other test classes) and the
        // MiscTypeCode is unique, so this is idempotent and never deletes them.
        private async Task<int> SeedSourceTypeAsync(string code = "GRN")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.Set<QCManagement.Domain.Entities.MiscTypeMaster>()
                .FirstOrDefaultAsync(t => t.MiscTypeCode == "QP_SOURCE_TYPE");
            if (type == null)
            {
                type = new QCManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "QP_SOURCE_TYPE",
                    Description = "QC Inspection Source Document Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.Set<QCManagement.Domain.Entities.MiscTypeMaster>().Add(type);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.Set<QCManagement.Domain.Entities.MiscMaster>()
                .FirstOrDefaultAsync(m => m.MiscTypeId == type.Id && m.Code == code);
            if (misc == null)
            {
                misc = new QCManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id,
                    Code = code,
                    Description = code,
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.Set<QCManagement.Domain.Entities.MiscMaster>().Add(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private static QcInspectionHdr BuildEntity(int sourceTypeId, string no, int sourceDetailId) =>
            new QcInspectionHdr
            {
                QcInspectionNo = no,
                InspectionDate = DateTimeOffset.UtcNow,
                SourceTypeId = sourceTypeId,
                SourceHeaderId = 100,
                SourceDetailId = sourceDetailId,
                QualitySpecificationId = 5,
                QualitySpecificationCode = "QS-0001",
                QualityTemplateId = 11,
                QualityTemplateCode = "QT-000001",
                QcTypeId = 8,
                InspectorUserId = 1,
                InspectorName = "tester",
                ReceivedQuantity = 1000m,
                ReceivedUomId = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedAsync(int sourceTypeId, string no = "QCI-2026-00001", int sourceDetailId = 4321)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new QcInspectionCommandRepository(
                ctx,
                new Mock<IGrnQcUpdate>(MockBehavior.Loose).Object,
                new Mock<IArrivalQcUpdate>(MockBehavior.Loose).Object);
            return await repo.CreateAsync(BuildEntity(sourceTypeId, no, sourceDetailId));
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("QC.QcInspectionDtl", "QC.QcInspectionHdr");

        [Fact]
        public async Task InspectionExistsForSourceAsync_Should_Be_True_After_Seed()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            await SeedAsync(stid, sourceDetailId: 4321);

            (await CreateRepo().InspectionExistsForSourceAsync(stid, 4321)).Should().BeTrue();
            (await CreateRepo().InspectionExistsForSourceAsync(stid, 9999)).Should().BeFalse();
        }

        [Fact]
        public async Task GetSourceTypeIdByCodeAsync_Should_Resolve_Seeded_Code()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync("GRN");

            (await CreateRepo().GetSourceTypeIdByCodeAsync("GRN")).Should().Be(stid);
        }

        [Fact]
        public async Task NotFoundAsync_Should_Be_True_When_Missing()
        {
            await ClearAsync();
            (await CreateRepo().NotFoundAsync(123456)).Should().BeTrue();
        }

        [Fact]
        public async Task GetMaxInspectionSequenceAsync_Should_Return_Highest()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            await SeedAsync(stid, "QCI-2026-00005", 4321);

            (await CreateRepo().GetMaxInspectionSequenceAsync(2026)).Should().Be(5);
        }

        [Fact]
        public async Task GetReceivedQuantityAsync_Should_Return_Value()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            var id = await SeedAsync(stid, sourceDetailId: 4321);

            (await CreateRepo().GetReceivedQuantityAsync(id)).Should().Be(1000m);
        }

        [Fact]
        public async Task IsDisposedAsync_Should_Be_False_For_Draft()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            var id = await SeedAsync(stid, sourceDetailId: 4321);

            (await CreateRepo().IsDisposedAsync(id)).Should().BeFalse();
        }

        // Seeds a generic QC.MiscMaster row (used for QualityParameter's FK columns) and returns its Id.
        private async Task<int> SeedMiscMasterAsync(string typeCode, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.Set<QCManagement.Domain.Entities.MiscTypeMaster>()
                .FirstOrDefaultAsync(t => t.MiscTypeCode == typeCode);
            if (type == null)
            {
                type = new QCManagement.Domain.Entities.MiscTypeMaster
                { MiscTypeCode = typeCode, Description = typeCode, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                ctx.Set<QCManagement.Domain.Entities.MiscTypeMaster>().Add(type);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.Set<QCManagement.Domain.Entities.MiscMaster>()
                .FirstOrDefaultAsync(m => m.MiscTypeId == type.Id && m.Code == code);
            if (misc == null)
            {
                misc = new QCManagement.Domain.Entities.MiscMaster
                { MiscTypeId = type.Id, Code = code, Description = code, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                ctx.Set<QCManagement.Domain.Entities.MiscMaster>().Add(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        // Seeds a QC.QualityParameter master row and returns its Id plus the MiscMaster id used for DataTypeId.
        private async Task<(int paramId, int dataTypeId)> SeedQualityParameterAsync(string code = "QP-000016", string name = "Trash %")
        {
            var miscId = await SeedMiscMasterAsync("QP_DATA_TYPE_TEST", $"DT-{code}");

            await using var ctx = _fixture.CreateFreshDbContext();
            var qp = new QCManagement.Domain.Entities.QualityParameter
            {
                ParameterCode = code,
                ParameterName = name,
                ParameterGroupId = miscId,
                DataTypeId = miscId,
                ValidationTypeId = miscId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<QCManagement.Domain.Entities.QualityParameter>().Add(qp);
            await ctx.SaveChangesAsync();
            return (qp.Id, miscId);
        }

        // Seeds a QC.QcInspectionDtl row (inspection parameter snapshot) for the given header.
        private async Task SeedDetailAsync(int hdrId, int qualityParameterId, string code = "QP-000016", string name = "Trash %")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.Set<QCManagement.Domain.Entities.QcInspectionDtl>().Add(new QCManagement.Domain.Entities.QcInspectionDtl
            {
                QcInspectionHdrId = hdrId,
                QualitySpecificationParameterId = 1,
                QualityParameterId = qualityParameterId,
                ParameterCode = code,
                ParameterName = name,
                DataTypeId = 7,
                ValidationTypeId = 11,
                ValidationTypeCode = "RNG",
                SeverityId = 1,
                SeverityCode = "MIN",
                FailureActionId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Set_OcrValue_On_Matching_Parameter()
        {
            await ClearAsync();
            await _fixture.ClearTablesAsync("QC.QualityParameter");
            var stid = await SeedSourceTypeAsync("ARRIVAL");
            var (paramId, _) = await SeedQualityParameterAsync("QP-000016", "Trash %");
            var inspectionId = await SeedAsync(stid, sourceDetailId: 4321);   // BuildEntity sets SourceHeaderId = 100
            await SeedDetailAsync(inspectionId, qualityParameterId: paramId);       // matches the OCR param
            await SeedDetailAsync(inspectionId, qualityParameterId: 999999, code: "QP-OTHER", name: "Other"); // no OCR match

            var ocrLookup = new Mock<IOcrQualityParameterLookup>(MockBehavior.Loose);
            ocrLookup
                .Setup(l => l.GetByArrivalHeaderIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OcrQualityParameterLookupDto>
                {
                    new() { ParamId = paramId, Value = "3.00%" }
                });

            var dto = await CreateRepo(ocrLookup.Object).GetByIdAsync(inspectionId);

            dto.Should().NotBeNull();
            dto!.Parameters.Should().HaveCount(2);
            dto.Parameters.Single(p => p.QualityParameterId == paramId).OcrValue.Should().Be("3.00%");
            dto.Parameters.Single(p => p.QualityParameterId == 999999).OcrValue.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Enrich_OcrQualityParameters_From_Master()
        {
            await ClearAsync();
            await _fixture.ClearTablesAsync("QC.QualityParameter");
            var stid = await SeedSourceTypeAsync("ARRIVAL");
            var (paramId, dataTypeId) = await SeedQualityParameterAsync("QP-000016", "Trash %");
            var inspectionId = await SeedAsync(stid, sourceDetailId: 4321);   // BuildEntity sets SourceHeaderId = 100

            var ocrLookup = new Mock<IOcrQualityParameterLookup>(MockBehavior.Loose);
            ocrLookup
                .Setup(l => l.GetByArrivalHeaderIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OcrQualityParameterLookupDto>
                {
                    new() { ParamId = paramId, Value = "3.00%" }
                });

            var dto = await CreateRepo(ocrLookup.Object).GetByIdAsync(inspectionId);

            dto.Should().NotBeNull();
            dto!.OcrQualityParameters.Should().ContainSingle();
            var p = dto.OcrQualityParameters.Single();
            p.QualityParameterId.Should().Be(paramId);
            p.ParameterCode.Should().Be("QP-000016");
            p.ParameterName.Should().Be("Trash %");
            p.DataTypeId.Should().Be(dataTypeId);
            p.Value.Should().Be("3.00%");
        }
    }
}

