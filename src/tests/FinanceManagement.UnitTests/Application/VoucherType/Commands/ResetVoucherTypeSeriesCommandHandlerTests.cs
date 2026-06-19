using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries;

namespace FinanceManagement.UnitTests.Application.VoucherType.Commands
{
    public sealed class ResetVoucherTypeSeriesCommandHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ResetVoucherTypeSeriesCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private static ResetVoucherTypeSeriesCommand ValidCommand() =>
            new() { VoucherTypeId = 1, FinancialYearId = 3 };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.ResetSeriesAsync(1, 3)).ReturnsAsync(10);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("reset successfully");
            result.Data.Should().Be(10);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsResetOnce()
        {
            _mockCommandRepo.Setup(r => r.ResetSeriesAsync(2, 4)).ReturnsAsync(1);

            await CreateSut().Handle(new ResetVoucherTypeSeriesCommand { VoucherTypeId = 2, FinancialYearId = 4 }, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.ResetSeriesAsync(2, 4), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.ResetSeriesAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "VOUCHER_TYPE_SERIES_RESET"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
