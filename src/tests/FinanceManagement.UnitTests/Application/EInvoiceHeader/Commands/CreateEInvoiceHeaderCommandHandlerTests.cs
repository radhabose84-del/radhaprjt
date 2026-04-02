using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Commands
{
    public sealed class CreateEInvoiceHeaderCommandHandlerTests
    {
        private readonly Mock<IEInvoiceHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateEInvoiceHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private CreateEInvoiceHeaderCommand ValidCommand() =>
            new()
            {
                UnitId = 1,
                DocType = "INV",
                SupplyType = "B2B",
                InvoiceNo = "INV001",
                InvoiceDate = new DateOnly(2026, 1, 15),
                PartyId = 5,
                GstNo = "29AAAAA1234A1Z5",
                InvoiceAmount = 10000m
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.EInvoiceHeader>(It.IsAny<CreateEInvoiceHeaderCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.EInvoiceHeader());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.EInvoiceHeader>()))
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
            SetupHappyPath(newId: 10);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Data.Should().Be(10);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.EInvoiceHeader>()),
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
                        e.ActionCode == "EINVOICE_HEADER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
