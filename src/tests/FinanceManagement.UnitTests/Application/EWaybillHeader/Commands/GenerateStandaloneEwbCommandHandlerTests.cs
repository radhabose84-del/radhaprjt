using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Application.EWaybillHeader.Commands.GenerateStandaloneEwb;

namespace FinanceManagement.UnitTests.Application.EWaybillHeader.Commands
{
    public sealed class GenerateStandaloneEwbCommandHandlerTests
    {
        private readonly Mock<INicEInvoiceService> _mockNicService = new(MockBehavior.Strict);
        private readonly Mock<IEWaybillHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GenerateStandaloneEwbCommandHandler CreateSut() =>
            new(_mockNicService.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private static GenerateStandaloneEwbCommand BuildCommand(int headerId = 100) =>
            new()
            {
                EWaybillHeaderId = headerId,
                Payload = new StandaloneEwbPayloadDto
                {
                    DocNo = "STODC/2026/0042",
                    DocDate = "08/05/2026",
                    FromGstin = "33AAACB8513A1ZE",
                    FromTrdName = "BASML",
                    ToGstin = "33AAACB8513A1ZE",
                    ToTrdName = "BASML",
                    TotalValue = 695m,
                    TotInvValue = 695m,
                    TransDistance = 50,
                    VehicleNo = "TN45CL9009",
                    ItemList = new()
                    {
                        new() { ItemNo = 1, ProductName = "Yarn", HsnCode = 5205, Quantity = 3m, QtyUnit = "KGS", TaxableAmount = 695m }
                    }
                }
            };

        // ---------------------------------------------------------------------------
        // NIC success path — row stamped with EWB number, status flipped to Generated
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task Handle_NicSuccess_CallsUpdateAfterNicSuccessWithEwbNumber()
        {
            var cmd = BuildCommand(headerId: 88);

            _mockNicService
                .Setup(s => s.GenerateStandaloneEwbAsync(cmd.Payload, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NicEwbResultDto
                {
                    IsSuccess    = true,
                    EwbNo        = 511009072762L,
                    EwbDate      = "08/05/2026 12:00:00 PM",
                    EwbValidTill = "09/05/2026 12:00:00 PM"
                });
            _mockCommandRepo
                .Setup(r => r.UpdateAfterNicSuccessAsync(
                    88, "511009072762", It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.EwbNo.Should().Be(511009072762L);

            _mockCommandRepo.Verify(
                r => r.UpdateAfterNicSuccessAsync(88, "511009072762",
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockCommandRepo.Verify(
                r => r.UpdateAfterNicFailureAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never,
                "failure update must NOT be called when NIC succeeds");
        }

        [Fact]
        public async Task Handle_NicSuccess_PublishesAuditEvent()
        {
            var cmd = BuildCommand(headerId: 88);

            _mockNicService
                .Setup(s => s.GenerateStandaloneEwbAsync(It.IsAny<StandaloneEwbPayloadDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NicEwbResultDto { IsSuccess = true, EwbNo = 1L });
            _mockCommandRepo
                .Setup(r => r.UpdateAfterNicSuccessAsync(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(cmd, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "STANDALONE_EWB_GENERATED"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------------------------------------------------------------------------
        // NIC failure path — row keeps Pending, ErrorCode/ErrorMessage stamped, no audit
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task Handle_NicFailure_CallsUpdateAfterNicFailureWithErrorDetails()
        {
            var cmd = BuildCommand(headerId: 99);

            _mockNicService
                .Setup(s => s.GenerateStandaloneEwbAsync(cmd.Payload, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NicEwbResultDto
                {
                    IsSuccess    = false,
                    ErrorCode    = "VALIDATION_ERROR",
                    ErrorMessage = "Vehicle Number Format Invalid"
                });
            _mockCommandRepo
                .Setup(r => r.UpdateAfterNicFailureAsync(99, "VALIDATION_ERROR", "Vehicle Number Format Invalid",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Vehicle Number Format Invalid");
            result.Data!.ErrorCode.Should().Be("VALIDATION_ERROR");

            _mockCommandRepo.Verify(
                r => r.UpdateAfterNicFailureAsync(99, "VALIDATION_ERROR", "Vehicle Number Format Invalid",
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _mockCommandRepo.Verify(
                r => r.UpdateAfterNicSuccessAsync(It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never,
                "success update must NOT be called when NIC fails");
        }

        [Fact]
        public async Task Handle_NicFailure_DoesNotPublishAuditEvent()
        {
            var cmd = BuildCommand(headerId: 99);

            _mockNicService
                .Setup(s => s.GenerateStandaloneEwbAsync(It.IsAny<StandaloneEwbPayloadDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NicEwbResultDto { IsSuccess = false, ErrorMessage = "fail" });
            _mockCommandRepo
                .Setup(r => r.UpdateAfterNicFailureAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(cmd, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never,
                "audit event for STANDALONE_EWB_GENERATED is reserved for actual successful generation");
        }
    }
}
