using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig;

namespace FinanceManagement.UnitTests.Application.CurrencyForexConfig.Commands
{
    public sealed class UpdateCurrencyForexConfigCommandHandlerTests
    {
        private readonly Mock<ICurrencyForexConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyForexConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateCurrencyForexConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateCurrencyForexConfigCommand ValidCommand(int isActive = 1) =>
            new() { Id = 1, CurrencyTypeName = "Forex", IsActive = isActive };

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.CurrencyForexConfig>(It.IsAny<UpdateCurrencyForexConfigCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.CurrencyForexConfig());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.CurrencyForexConfig>()))
                .ReturnsAsync(updatedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated successfully");
        }

        [Fact]
        public async Task Handle_Inactivate_WhenLinked_Throws()
        {
            _mockQueryRepo.Setup(r => r.IsCurrencyForexConfigLinkedAsync(1)).ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*cannot inactivate*");
        }

        [Fact]
        public async Task Handle_Inactivate_WhenNotLinked_Succeeds()
        {
            _mockQueryRepo.Setup(r => r.IsCurrencyForexConfigLinkedAsync(1)).ReturnsAsync(false);
            SetupHappyPath();

            var result = await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
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
                        e.ActionCode == "CURRENCY_FOREX_CONFIG_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
