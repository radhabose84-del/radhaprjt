using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CancelIrn;
using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Commands
{
    public sealed class CancelIrnCommandHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CancelIrnCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockNicService.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_SuccessfulCancel_ReturnsSuccess()
        {
            var nicResult = new NicCancelIrnResultDto
            {
                IsSuccess = true,
                Irn = "IRN123",
                CancelDate = "2026-01-15"
            };

            _mockNicService
                .Setup(s => s.CancelIrnAsync(1, "1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(1, null, null, null, null, null,
                    "Cancelled", null, It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new CancelIrnCommand { EInvoiceHeaderId = 1, CnlRsn = "1" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("cancelled successfully");
        }

        [Fact]
        public async Task Handle_FailedCancel_ReturnsFailure()
        {
            var nicResult = new NicCancelIrnResultDto
            {
                IsSuccess = false,
                ErrorCode = "ERR002",
                ErrorMessage = "Cancellation failed"
            };

            _mockNicService
                .Setup(s => s.CancelIrnAsync(1, "1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnStatusAsync(1, "CancelFailed", "ERR002", "Cancellation failed",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new CancelIrnCommand { EInvoiceHeaderId = 1, CnlRsn = "1" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var nicResult = new NicCancelIrnResultDto
            {
                IsSuccess = true,
                CancelDate = "2026-01-15"
            };

            _mockNicService
                .Setup(s => s.CancelIrnAsync(1, "1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(
                new CancelIrnCommand { EInvoiceHeaderId = 1, CnlRsn = "1" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "EINVOICE_CANCEL_IRN"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullCnlRsn_DefaultsTo1()
        {
            var nicResult = new NicCancelIrnResultDto { IsSuccess = true, CancelDate = "2026-01-15" };

            _mockNicService
                .Setup(s => s.CancelIrnAsync(1, "1", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(
                new CancelIrnCommand { EInvoiceHeaderId = 1, CnlRsn = null },
                CancellationToken.None);

            _mockNicService.Verify(
                s => s.CancelIrnAsync(1, "1", It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
