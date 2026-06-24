using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.RunGapScan;

namespace FinanceManagement.UnitTests.Application.JournalThresholdRule
{
    public sealed class RunGapScanCommandHandlerTests
    {
        private readonly Mock<IGapScanService> _service = new(MockBehavior.Strict);

        private RunGapScanCommandHandler CreateSut() => new(_service.Object);

        [Fact]
        public async Task Handle_NoGaps_ReturnsZero_SuccessMessage()
        {
            _service.Setup(s => s.ScanAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            var result = await CreateSut().Handle(new RunGapScanCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(0);
            result.Message.Should().Contain("no missing");
        }

        [Fact]
        public async Task Handle_GapsFound_ReturnsCount()
        {
            _service.Setup(s => s.ScanAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);

            var result = await CreateSut().Handle(new RunGapScanCommand(), CancellationToken.None);

            result.Data.Should().Be(3);
            result.Message.Should().Contain("3 missing");
            _service.Verify(s => s.ScanAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
