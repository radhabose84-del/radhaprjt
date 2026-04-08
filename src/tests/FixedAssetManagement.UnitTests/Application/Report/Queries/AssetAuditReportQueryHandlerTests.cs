using AutoMapper;
using FAM.Application.Common.Interfaces.IReports;
using FAM.Application.Reports.AssetAudit;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Report.Queries
{
    public sealed class AssetAuditReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private AssetAuditReportQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithData_ReturnsSuccess()
        {
            var reports = new List<AssetAuditReportDto>
            {
                new AssetAuditReportDto { Audit_AssetCode = "AST001", AuditorName = "Tester" }
            };

            _mockRepo
                .Setup(r => r.AssetAuditReportAsync(1))
                .ReturnsAsync(reports);

            _mockMapper
                .Setup(m => m.Map<List<AssetAuditReportDto>>(It.IsAny<object>()))
                .Returns(reports);

            var result = await CreateSut().Handle(
                new AssetAuditReportQuery { AuditCycle = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyData_ReturnsSuccessWithEmptyList()
        {
            _mockRepo
                .Setup(r => r.AssetAuditReportAsync(1))
                .ReturnsAsync(new List<AssetAuditReportDto>());

            _mockMapper
                .Setup(m => m.Map<List<AssetAuditReportDto>>(It.IsAny<object>()))
                .Returns(new List<AssetAuditReportDto>());

            var result = await CreateSut().Handle(
                new AssetAuditReportQuery { AuditCycle = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.AssetAuditReportAsync(1))
                .ReturnsAsync(new List<AssetAuditReportDto>());

            _mockMapper
                .Setup(m => m.Map<List<AssetAuditReportDto>>(It.IsAny<object>()))
                .Returns(new List<AssetAuditReportDto>());

            await CreateSut().Handle(
                new AssetAuditReportQuery { AuditCycle = 1 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
