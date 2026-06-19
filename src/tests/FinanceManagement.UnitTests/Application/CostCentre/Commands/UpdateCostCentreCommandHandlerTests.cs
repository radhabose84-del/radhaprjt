using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre;

namespace FinanceManagement.UnitTests.Application.CostCentre.Commands
{
    public sealed class UpdateCostCentreCommandHandlerTests
    {
        private readonly Mock<ICostCentreCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICostCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateCostCentreCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateCostCentreCommand ValidCommand(int isActive = 1) =>
            new() { Id = 1, CostCentreName = "Plant Edited", IsActive = isActive };

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.CostCentre>(It.IsAny<UpdateCostCentreCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.CostCentre());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.CostCentre>()))
                .ReturnsAsync(updatedId);
            _mockQueryRepo.Setup(r => r.HasOpenTransactionsAsync(It.IsAny<int>())).ReturnsAsync(false);
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
        public async Task Handle_Deactivate_WithNoOpenTransactions_Succeeds()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.HasOpenTransactionsAsync(1)).ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Deactivate_WithOpenTransactions_Throws()
        {
            _mockQueryRepo.Setup(r => r.HasOpenTransactionsAsync(1)).ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(isActive: 0), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*open transactions*");
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
                        e.ActionCode == "COST_CENTRE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
