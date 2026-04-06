using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CancelEwb;
using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Commands
{
    public sealed class CancelEwbCommandHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CancelEwbCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockNicService.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_SuccessfulCancel_ReturnsSuccess()
        {
            var nicResult = new NicCancelEwbResultDto
            {
                IsSuccess = true,
                EwbNo = 123456789L,
                CancelDate = "2026-01-15"
            };

            _mockNicService
                .Setup(s => s.CancelEwbAsync(1, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(1, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new CancelEwbCommand { EInvoiceHeaderId = 1, CancelRsnCode = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("cancelled successfully");
        }

        [Fact]
        public async Task Handle_FailedCancel_ReturnsFailure()
        {
            var nicResult = new NicCancelEwbResultDto
            {
                IsSuccess = false,
                ErrorCode = "ERR004",
                ErrorMessage = "EWB cancellation failed"
            };

            _mockNicService
                .Setup(s => s.CancelEwbAsync(1, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            var result = await CreateSut().Handle(
                new CancelEwbCommand { EInvoiceHeaderId = 1, CancelRsnCode = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_SuccessfulCancel_ClearsEwbDetails()
        {
            var nicResult = new NicCancelEwbResultDto
            {
                IsSuccess = true,
                CancelDate = "2026-01-15"
            };

            _mockNicService
                .Setup(s => s.CancelEwbAsync(1, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(1, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(
                new CancelEwbCommand { EInvoiceHeaderId = 1, CancelRsnCode = 1 },
                CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateEwbDetailsAsync(1, null, null, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FailedCancel_DoesNotClearEwbDetails()
        {
            var nicResult = new NicCancelEwbResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Failed"
            };

            _mockNicService
                .Setup(s => s.CancelEwbAsync(1, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            await CreateSut().Handle(
                new CancelEwbCommand { EInvoiceHeaderId = 1, CancelRsnCode = 1 },
                CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateEwbDetailsAsync(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var nicResult = new NicCancelEwbResultDto { IsSuccess = true, CancelDate = "2026-01-15" };

            _mockNicService
                .Setup(s => s.CancelEwbAsync(1, 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(1, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(
                new CancelEwbCommand { EInvoiceHeaderId = 1, CancelRsnCode = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "EINVOICE_CANCEL_EWB"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
