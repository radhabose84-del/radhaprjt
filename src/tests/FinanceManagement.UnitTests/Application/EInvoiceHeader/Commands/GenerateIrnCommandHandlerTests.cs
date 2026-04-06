using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn;
using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Commands
{
    public sealed class GenerateIrnCommandHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GenerateIrnCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockNicService.Object, _mockMediator.Object);

        private GenerateIrnCommand ValidCommand() =>
            new() { EInvoiceHeaderId = 1 };

        [Fact]
        public async Task Handle_SuccessfulIrn_ReturnsSuccess()
        {
            var nicResult = new NicIrnResultDto
            {
                IsSuccess = true,
                Irn = "IRN123",
                AckNo = "ACK001",
                AckDate = DateTimeOffset.UtcNow,
                SignedInvoice = "signed",
                SignedQRCode = "qr"
            };

            _mockNicService
                .Setup(s => s.GenerateIrnAsync(1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(1, "IRN123", "ACK001",
                    It.IsAny<DateTimeOffset?>(), "signed", "qr", "Generated", null, null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("IRN generated successfully");
            result.Data.Should().NotBeNull();
            result.Data!.Irn.Should().Be("IRN123");
        }

        [Fact]
        public async Task Handle_FailedIrn_ReturnsFailure()
        {
            var nicResult = new NicIrnResultDto
            {
                IsSuccess = false,
                ErrorCode = "ERR001",
                ErrorMessage = "Generation failed"
            };

            _mockNicService
                .Setup(s => s.GenerateIrnAsync(1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(1, null, null, null, null, null,
                    "Failed", "ERR001", "Generation failed",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_SuccessfulIrn_PublishesAuditEvent()
        {
            var nicResult = new NicIrnResultDto
            {
                IsSuccess = true,
                Irn = "IRN123",
                AckNo = "ACK001"
            };

            _mockNicService
                .Setup(s => s.GenerateIrnAsync(1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionCode == "EINVOICE_GENERATE_IRN"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_IrnWithEwb_UpdatesEwbDetails()
        {
            var nicResult = new NicIrnResultDto
            {
                IsSuccess = true,
                Irn = "IRN123",
                AckNo = "ACK001",
                EwbNo = 123456789L,
                EwbDate = "2026-01-15",
                EwbValidTill = "2026-01-18"
            };

            _mockNicService
                .Setup(s => s.GenerateIrnAsync(1, It.IsAny<EwbTransportDetails?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateIrnDetailsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(1, 123456789L, "2026-01-15", "2026-01-18",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new GenerateIrnCommand
            {
                EInvoiceHeaderId = 1,
                Distance = 100,
                TransMode = "1",
                VehNo = "KA01AB1234"
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("e-Waybill");
            _mockCommandRepo.Verify(
                r => r.UpdateEwbDetailsAsync(1, 123456789L, "2026-01-15", "2026-01-18",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
