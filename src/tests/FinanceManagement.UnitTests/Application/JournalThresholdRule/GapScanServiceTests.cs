using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Services;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Application.JournalThresholdRule
{
    public sealed class GapScanServiceTests
    {
        private readonly Mock<IGapScanRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);

        private GapScanService CreateSut()
        {
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            return new(_mockRepo.Object, _mockTz.Object);
        }

        private void SetupSeries(int lastUsed) =>
            _mockRepo.Setup(r => r.GetActiveSeriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NumberSeriesInfo> { new() { SeriesId = 10, VoucherTypeId = 1, FinancialYearId = 3, LastUsedNumber = lastUsed } });

        private SequenceGapScanLog? CaptureLog()
        {
            SequenceGapScanLog? captured = null;
            _mockRepo.Setup(r => r.AddScanLogAsync(It.IsAny<SequenceGapScanLog>(), It.IsAny<CancellationToken>()))
                .Callback<SequenceGapScanLog, CancellationToken>((l, _) => captured = l)
                .Returns(Task.CompletedTask);
            return captured;
        }

        [Fact]
        public async Task ContinuousSequence_NoGaps_NoAlert()
        {
            SetupSeries(lastUsed: 3);
            _mockRepo.Setup(r => r.GetUsedVoucherNumbersAsync(1, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { "JV/2026-27/0001", "JV/2026-27/0002", "JV/2026-27/0003" });
            _ = CaptureLog();

            var total = await CreateSut().ScanAllAsync(CancellationToken.None);

            total.Should().Be(0);
            _mockRepo.Verify(r => r.AddScanLogAsync(It.Is<SequenceGapScanLog>(l => l.GapsFound == 0 && !l.Alerted), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MissingNumber_DetectsGap_AndAlerts()
        {
            SetupSeries(lastUsed: 3);
            _mockRepo.Setup(r => r.GetUsedVoucherNumbersAsync(1, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { "JV/2026-27/0001", "JV/2026-27/0003" });   // 2 is missing
            _ = CaptureLog();

            var total = await CreateSut().ScanAllAsync(CancellationToken.None);

            total.Should().Be(1);
            _mockRepo.Verify(r => r.AddScanLogAsync(
                It.Is<SequenceGapScanLog>(l => l.GapsFound == 1 && l.Alerted && l.GapNumbers == "2"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MultipleGaps_AreAllReported()
        {
            SetupSeries(lastUsed: 5);
            _mockRepo.Setup(r => r.GetUsedVoucherNumbersAsync(1, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { "JV/2026-27/0001", "JV/2026-27/0004" });   // missing 2,3,5
            _ = CaptureLog();

            var total = await CreateSut().ScanAllAsync(CancellationToken.None);

            total.Should().Be(3);
            _mockRepo.Verify(r => r.AddScanLogAsync(It.Is<SequenceGapScanLog>(l => l.GapNumbers == "2,3,5"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task NoSeries_ReturnsZero_NoLog()
        {
            _mockRepo.Setup(r => r.GetActiveSeriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<NumberSeriesInfo>());

            var total = await CreateSut().ScanAllAsync(CancellationToken.None);

            total.Should().Be(0);
            _mockRepo.Verify(r => r.AddScanLogAsync(It.IsAny<SequenceGapScanLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
