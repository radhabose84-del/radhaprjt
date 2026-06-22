using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre;

namespace FinanceManagement.UnitTests.Application.ProfitCentre.Commands
{
    public sealed class UpdateProfitCentreCommandHandlerTests
    {
        private readonly Mock<IProfitCentreCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProfitCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateProfitCentreCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateProfitCentreCommand ValidCommand(int isActive = 1) =>
            new() { Id = 1, ProfitCentreName = "Spinning Edited", IsActive = isActive };

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ProfitCentre>(It.IsAny<UpdateProfitCentreCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ProfitCentre());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.ProfitCentre>()))
                .ReturnsAsync(updatedId);
            _mockQueryRepo.Setup(r => r.HasCurrentYearTransactionsAsync(It.IsAny<int>())).ReturnsAsync(false);
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
        public async Task Handle_Deactivate_WithNoCurrentYearTransactions_Succeeds()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.HasCurrentYearTransactionsAsync(1)).ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Deactivate_WithCurrentYearTransactions_Throws()
        {
            _mockQueryRepo.Setup(r => r.HasCurrentYearTransactionsAsync(1)).ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*year-end close*");
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
                        e.ActionCode == "PROFIT_CENTRE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
