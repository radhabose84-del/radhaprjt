using Contracts.Interfaces.Updates.Purchase;
using Microsoft.EntityFrameworkCore;
using QCManagement.Domain.Entities;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.QcInspection;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QcInspection
{
    [Collection("DatabaseCollection")]
    public sealed class QcInspectionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QcInspectionCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static QcInspectionCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx,
                new Mock<IGrnQcUpdate>(MockBehavior.Loose).Object,
                new Mock<IArrivalQcUpdate>(MockBehavior.Loose).Object);

        // SourceTypeId is FK to QC.MiscMaster — get-or-create a source-type row and return its Id.
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

        private static QcInspectionHdr BuildEntity(int sourceTypeId, string no = "QCI-2026-00001", int sourceDetailId = 4321) =>
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
                IsDeleted = IsDelete.NotDeleted,
                Details = new List<QcInspectionDtl>
                {
                    new QcInspectionDtl
                    {
                        QualitySpecificationParameterId = 501,
                        QualityParameterId = 1,
                        ParameterCode = "QP-1",
                        ParameterName = "Tensile",
                        DataTypeId = 2,
                        ValidationTypeId = 3,
                        ValidationTypeCode = "RNG",
                        MinValue = 10m,
                        MaxValue = 50m,
                        SeverityId = 6,
                        SeverityCode = "CRT",
                        FailureActionId = 7,
                        SortOrder = 1,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("QC.QcInspectionDtl", "QC.QcInspectionHdr");

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(stid));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(stid));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QcInspectionHdr.Include(h => h.Details).FirstAsync(x => x.Id == id);

            saved.QcInspectionNo.Should().Be("QCI-2026-00001");
            saved.Details.Should().HaveCount(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task SaveParameterResultsAsync_Should_Update_Detail()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(stid);
            var id = await CreateRepo(ctx).CreateAsync(entity);
            var detailId = entity.Details.First().Id;
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SaveParameterResultsAsync(id,
                new List<(int, string?, string?, string?)> { (detailId, "40", "PASS", "ok") });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QcInspectionDtl.FirstAsync(x => x.Id == detailId);
            saved.ActualValue.Should().Be("40");
            saved.InspectionResult.Should().Be("PASS");
        }

        [Fact]
        public async Task SaveResultsAndDispositionAsync_Should_Persist_Readings_And_Disposition()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            // QcStatusId is FK to QC.MiscMaster — seed a valid status misc row and use its Id.
            var statusId = await SeedSourceTypeAsync("APR");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(stid);
            var id = await CreateRepo(ctx).CreateAsync(entity);
            var detailId = entity.Details.First().Id;
            ctx.ChangeTracker.Clear();

            // GRN source → write-back goes through the (no-op) IGrnQcUpdate mock; header disposition persists.
            await CreateRepo(ctx).SaveResultsAndDispositionAsync(
                id,
                new List<(int, string?, string?, string?)> { (detailId, "40", "PASS", "ok") },
                qcStatusId: statusId, acceptedQty: 1000m, rejectedQty: 0m, dispositionRemarks: "approved",
                dispositionByUserId: 1, dispositionByName: "tester",
                qcApprovedIp: "127.0.0.1", isQcApproved: true,
                sourceTypeCode: "GRN", sourceHeaderId: 100, sourceDetailId: 4321, arrivalStatusName: "Approved");
            ctx.ChangeTracker.Clear();

            var hdr = await ctx.QcInspectionHdr.FirstAsync(x => x.Id == id);
            hdr.QcStatusId.Should().Be(statusId);
            hdr.AcceptedQuantity.Should().Be(1000m);
            hdr.DispositionByName.Should().Be("tester");

            var dtl = await ctx.QcInspectionDtl.FirstAsync(x => x.Id == detailId);
            dtl.ActualValue.Should().Be("40");
            dtl.InspectionResult.Should().Be("PASS");
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_When_Draft()
        {
            await ClearAsync();
            var stid = await SeedSourceTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(stid));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var deleted = await ctx.QcInspectionHdr.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
