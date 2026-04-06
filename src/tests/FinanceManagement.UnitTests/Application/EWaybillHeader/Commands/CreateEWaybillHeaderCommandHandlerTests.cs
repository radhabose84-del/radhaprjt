using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;

namespace FinanceManagement.UnitTests.Application.EWaybillHeader.Commands
{
    public sealed class CreateEWaybillHeaderCommandHandlerTests
    {
        private readonly Mock<IEWaybillHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateEWaybillHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private CreateEWaybillHeaderCommand ValidCommand() =>
            new()
            {
                UnitId = 1,
                EWBNumber = "EWB001",
                InvoiceNo = "INV001",
                InvoiceDate = new DateOnly(2026, 1, 15),
                InvoiceValue = 50000m,
                SupplyType = "Outward",
                TotalValue = 45000m,
                CGST = 2500m,
                SGST = 2500m,
                EwbStatus = "Pending"
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.EWaybillHeader>(It.IsAny<CreateEWaybillHeaderCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.EWaybillHeader());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.EWaybillHeader>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 25);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Data.Should().Be(25);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.EWaybillHeader>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "EWAYBILL_HEADER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity()
        {
            SetupHappyPath();
            var command = ValidCommand();
            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<FinanceManagement.Domain.Entities.EWaybillHeader>(command),
                Times.Once);
        }
    }
}
