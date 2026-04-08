using AutoMapper;
using FAM.Application.Common.Interfaces.IReports;
using FAM.Application.Reports.AssetReport;

namespace FixedAssetManagement.UnitTests.Application.Report.Queries
{
    public sealed class AssetReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private AssetReportQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_WithData_ReturnsSuccessTrue()
        {
            var reportEntities = new List<AssetReportDto>
            {
                new AssetReportDto { AssetCode = "AST001", AssetName = "Laptop" }
            };

            _mockRepo
                .Setup(r => r.AssetReportAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(reportEntities);

            _mockMapper
                .Setup(m => m.Map<List<AssetReportDto>>(It.IsAny<object>()))
                .Returns(reportEntities);

            var result = await CreateSut().Handle(
                new AssetReportQuery
                {
                    FromDate = DateTimeOffset.UtcNow.AddMonths(-1),
                    ToDate = DateTimeOffset.UtcNow
                },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoData_ReturnsSuccessFalse()
        {
            _mockRepo
                .Setup(r => r.AssetReportAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(new List<AssetReportDto>());

            _mockMapper
                .Setup(m => m.Map<List<AssetReportDto>>(It.IsAny<object>()))
                .Returns(new List<AssetReportDto>());

            var result = await CreateSut().Handle(
                new AssetReportQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("No Asset Report found.");
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.AssetReportAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(new List<AssetReportDto>());

            _mockMapper
                .Setup(m => m.Map<List<AssetReportDto>>(It.IsAny<object>()))
                .Returns(new List<AssetReportDto>());

            await CreateSut().Handle(new AssetReportQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.AssetReportAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        }
    }
}
