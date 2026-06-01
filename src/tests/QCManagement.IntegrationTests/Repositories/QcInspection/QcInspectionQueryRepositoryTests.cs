using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Updates.Purchase;
using Microsoft.Data.SqlClient;
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
            new(
                new SqlConnection(_fixture.ConnectionString),
                new Mock<IGrnLookup>(MockBehavior.Loose).Object,
                new Mock<IItemLookup>(MockBehavior.Loose).Object,
                new Mock<ISupplierLookup>(MockBehavior.Loose).Object,
                new Mock<IInventoryCategoryLookup>(MockBehavior.Loose).Object);

        private static QcInspectionHdr BuildEntity(string no, int grnDetailId) =>
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
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedAsync(string no = "QCI-2026-00001", int grnDetailId = 4321)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new QcInspectionCommandRepository(ctx, new Mock<IGrnQcUpdate>(MockBehavior.Loose).Object);
            return await repo.CreateAsync(BuildEntity(no, grnDetailId));
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("QC.QcInspectionDtl", "QC.QcInspectionHdr");

        [Fact]
        public async Task InspectionExistsForGrnDetailAsync_Should_Be_True_After_Seed()
        {
            await ClearAsync();
            await SeedAsync(grnDetailId: 4321);

            (await CreateRepo().InspectionExistsForGrnDetailAsync(4321)).Should().BeTrue();
            (await CreateRepo().InspectionExistsForGrnDetailAsync(9999)).Should().BeFalse();
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
            await SeedAsync("QCI-2026-00005", 4321);

            (await CreateRepo().GetMaxInspectionSequenceAsync(2026)).Should().Be(5);
        }

        [Fact]
        public async Task GetReceivedQuantityAsync_Should_Return_Value()
        {
            await ClearAsync();
            var id = await SeedAsync(grnDetailId: 4321);

            (await CreateRepo().GetReceivedQuantityAsync(id)).Should().Be(1000m);
        }

        [Fact]
        public async Task IsDisposedAsync_Should_Be_False_For_Draft()
        {
            await ClearAsync();
            var id = await SeedAsync(grnDetailId: 4321);

            (await CreateRepo().IsDisposedAsync(id)).Should().BeFalse();
        }
    }
}
