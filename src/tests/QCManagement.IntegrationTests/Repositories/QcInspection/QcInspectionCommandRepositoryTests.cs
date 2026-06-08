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
            new(ctx, new Mock<IGrnQcUpdate>(MockBehavior.Loose).Object);

        private static QcInspectionHdr BuildEntity(string no = "QCI-2026-00001", int grnDetailId = 4321) =>
            new QcInspectionHdr
            {
                QcInspectionNo = no,
                InspectionDate = DateTimeOffset.UtcNow,
                GrnHeaderId = 100,
                GrnDetailId = grnDetailId,
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

        // QcStatusId has a real DB FK to QC.MiscMaster — seed a status row and return its Id.
        private static async Task<int> SeedQcStatusAsync(ApplicationDbContext ctx)
        {
            var miscType = new QCManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "QC_STATUS",
                Description = "QC Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<QCManagement.Domain.Entities.MiscTypeMaster>().Add(miscType);
            await ctx.SaveChangesAsync();

            var status = new QCManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "APPROVED",
                Description = "Approved",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<QCManagement.Domain.Entities.MiscMaster>().Add(status);
            await ctx.SaveChangesAsync();

            return status.Id;
        }

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());
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
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity();
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
            await using var ctx = _fixture.CreateFreshDbContext();
            var qcStatusId = await SeedQcStatusAsync(ctx);
            var entity = BuildEntity();
            var id = await CreateRepo(ctx).CreateAsync(entity);
            var detailId = entity.Details.First().Id;
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SaveResultsAndDispositionAsync(
                id,
                new List<(int, string?, string?, string?)> { (detailId, "40", "PASS", "ok") },
                qcStatusId: qcStatusId, acceptedQty: 1000m, rejectedQty: 0m, dispositionRemarks: "approved",
                dispositionByUserId: 1, dispositionByName: "tester",
                qcApprovedIp: "127.0.0.1", isQcApproved: true,
                grnHeaderId: 100, grnDetailId: 4321);
            ctx.ChangeTracker.Clear();

            var hdr = await ctx.QcInspectionHdr.FirstAsync(x => x.Id == id);
            hdr.QcStatusId.Should().Be(qcStatusId);
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
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var deleted = await ctx.QcInspectionHdr.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
