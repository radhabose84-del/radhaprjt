using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;

namespace FinanceManagement.UnitTests.Application.TransactionTypeMaster.Commands
{
    public sealed class UpdateTransactionTypeMasterCommandHandlerTests
    {
        private readonly Mock<ITransactionTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateTransactionTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private UpdateTransactionTypeMasterCommand ValidCommand() =>
            new()
            {
                Id = 1,
                UnitId = 1,
                ModuleId = 2,
                MenuId = 3,
                TypeName = "Updated Invoice",
                ShortName = "INV",
                Description = "Updated Description",
                IsActive = 1
            };

        private void SetupHappyPath(int result = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TransactionTypeMaster>(It.IsAny<UpdateTransactionTypeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TransactionTypeMaster { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()))
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdateResult()
        {
            SetupHappyPath(result: 1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()),
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
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "TRANSACTION_TYPE_UPDATE"),
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
    }
}
