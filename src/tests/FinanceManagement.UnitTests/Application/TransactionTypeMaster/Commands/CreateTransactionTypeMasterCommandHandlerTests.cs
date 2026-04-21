using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;

namespace FinanceManagement.UnitTests.Application.TransactionTypeMaster.Commands
{
    public sealed class CreateTransactionTypeMasterCommandHandlerTests
    {
        private readonly Mock<ITransactionTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateTransactionTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockIp.Object);

        private CreateTransactionTypeMasterCommand ValidCommand() =>
            new()
            {
                ModuleId = 2,
                MenuId = 3,
                TypeName = "Invoice",
                ShortName = "INV",
                Description = "Invoice Type"
            };

        private void SetupHappyPath(int newId = 1, int unitId = 1)
        {
            _mockIp.Setup(x => x.GetUnitId()).Returns(unitId);

            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TransactionTypeMaster>(It.IsAny<CreateTransactionTypeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TransactionTypeMaster());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()),
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
                        e.ActionCode == "TRANSACTION_TYPE_CREATE"),
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
                m => m.Map<FinanceManagement.Domain.Entities.TransactionTypeMaster>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_StampsUnitIdFromToken()
        {
            FinanceManagement.Domain.Entities.TransactionTypeMaster? captured = null;
            _mockIp.Setup(x => x.GetUnitId()).Returns(7);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TransactionTypeMaster>(It.IsAny<CreateTransactionTypeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TransactionTypeMaster());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()))
                .Callback<FinanceManagement.Domain.Entities.TransactionTypeMaster>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.UnitId.Should().Be(7);
        }

        [Fact]
        public async Task Handle_NoUnitClaim_StampsZeroUnitId()
        {
            FinanceManagement.Domain.Entities.TransactionTypeMaster? captured = null;
            _mockIp.Setup(x => x.GetUnitId()).Returns((int?)null);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TransactionTypeMaster>(It.IsAny<CreateTransactionTypeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TransactionTypeMaster());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()))
                .Callback<FinanceManagement.Domain.Entities.TransactionTypeMaster>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured!.UnitId.Should().Be(0);
        }
    }
}
