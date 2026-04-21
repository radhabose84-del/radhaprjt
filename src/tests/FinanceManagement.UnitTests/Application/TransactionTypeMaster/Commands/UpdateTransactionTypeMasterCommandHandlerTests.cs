using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;

namespace FinanceManagement.UnitTests.Application.TransactionTypeMaster.Commands
{
    public sealed class UpdateTransactionTypeMasterCommandHandlerTests
    {
        private readonly Mock<ITransactionTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ITransactionTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateTransactionTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateTransactionTypeMasterCommand ValidCommand(int isActive = 1) =>
            new()
            {
                Id = 1,
                ModuleId = 2,
                MenuId = 3,
                TypeName = "Updated Invoice",
                ShortName = "UINV",
                Description = "Updated Invoice Type",
                IsActive = isActive
            };

        private void SetupHappyPath()
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TransactionTypeMaster>(It.IsAny<UpdateTransactionTypeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TransactionTypeMaster());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.TransactionTypeMaster>()))
                .ReturnsAsync(1);
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
        public async Task Handle_InactivateLinkedRecord_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.IsTransactionTypeMasterLinkedAsync(1))
                .ReturnsAsync(true);

            Func<Task> act = async () =>
                await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*inactivate*");
        }
    }
}
