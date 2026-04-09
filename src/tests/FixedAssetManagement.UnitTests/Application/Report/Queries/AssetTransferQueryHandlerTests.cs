using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.IReports;
using FAM.Application.Reports.AssetTransferReport;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Report.Queries
{
    public sealed class AssetTransferQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private AssetTransferQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_WithData_ReturnsSuccess()
        {
            var reports = new List<AssetTransferDetailsDto>
            {
                new AssetTransferDetailsDto { TransferId = 1, AssetCode = "AST001" }
            };

            _mockRepo
                .Setup(r => r.AssetTransferReportAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(reports);

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferDetailsDto>>(It.IsAny<object>()))
                .Returns(reports);

            var result = await CreateSut().Handle(
                new AssetTransferQuery
                {
                    FromDate = DateTimeOffset.UtcNow.AddMonths(-1),
                    ToDate = DateTimeOffset.UtcNow
                },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullFromDate_ThrowsArgumentNullException()
        {
            var query = new AssetTransferQuery
            {
                FromDate = null,
                ToDate = DateTimeOffset.UtcNow
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateSut().Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NullToDate_ThrowsArgumentNullException()
        {
            var query = new AssetTransferQuery
            {
                FromDate = DateTimeOffset.UtcNow.AddMonths(-1),
                ToDate = null
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateSut().Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var reports = new List<AssetTransferDetailsDto>
            {
                new AssetTransferDetailsDto { TransferId = 1 }
            };

            _mockRepo
                .Setup(r => r.AssetTransferReportAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(reports);

            _mockMapper
                .Setup(m => m.Map<List<AssetTransferDetailsDto>>(It.IsAny<object>()))
                .Returns(reports);

            await CreateSut().Handle(
                new AssetTransferQuery
                {
                    FromDate = DateTimeOffset.UtcNow.AddMonths(-1),
                    ToDate = DateTimeOffset.UtcNow
                },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
