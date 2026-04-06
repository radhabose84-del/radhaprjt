using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb;
using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Commands
{
    public sealed class GenerateEwbCommandHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GenerateEwbCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockNicService.Object, _mockMediator.Object);

        private GenerateEwbCommand ValidCommand() =>
            new()
            {
                EInvoiceHeaderId = 1,
                TransporterId = "29TTTTT9999T1Z5",
                TransporterName = "Transport Co",
                TransMode = "1",
                Distance = 100,
                VehicleNo = "KA01AB1234",
                VehicleType = "R"
            };

        [Fact]
        public async Task Handle_SuccessfulGeneration_ReturnsSuccess()
        {
            var nicResult = new NicEwbResultDto
            {
                IsSuccess = true,
                EwbNo = 123456789L,
                EwbDate = "2026-01-15",
                EwbValidTill = "2026-01-18"
            };

            _mockNicService
                .Setup(s => s.GenerateEwbAsync(1, "29TTTTT9999T1Z5", "Transport Co", "1", 100,
                    It.IsAny<string?>(), It.IsAny<string?>(), "KA01AB1234", "R",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(1, 123456789L, "2026-01-15", "2026-01-18",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("generated successfully");
        }

        [Fact]
        public async Task Handle_FailedGeneration_ReturnsFailure()
        {
            var nicResult = new NicEwbResultDto
            {
                IsSuccess = false,
                ErrorCode = "ERR003",
                ErrorMessage = "EWB generation failed"
            };

            _mockNicService
                .Setup(s => s.GenerateEwbAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_SuccessfulGeneration_UpdatesEwbDetails()
        {
            var nicResult = new NicEwbResultDto
            {
                IsSuccess = true,
                EwbNo = 999L,
                EwbDate = "2026-01-15",
                EwbValidTill = "2026-01-18"
            };

            _mockNicService
                .Setup(s => s.GenerateEwbAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(1, 999L, "2026-01-15", "2026-01-18",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateEwbDetailsAsync(1, 999L, "2026-01-15", "2026-01-18",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var nicResult = new NicEwbResultDto { IsSuccess = true, EwbNo = 999L };

            _mockNicService
                .Setup(s => s.GenerateEwbAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(nicResult);

            _mockCommandRepo
                .Setup(r => r.UpdateEwbDetailsAsync(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "EINVOICE_GENERATE_EWB"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
