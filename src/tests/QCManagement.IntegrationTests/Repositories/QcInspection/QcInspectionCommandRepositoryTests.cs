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
