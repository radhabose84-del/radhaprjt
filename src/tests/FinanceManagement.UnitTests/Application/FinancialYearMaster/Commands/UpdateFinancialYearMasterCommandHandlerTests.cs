using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Commands
{
    public sealed class UpdateFinancialYearMasterCommandHandlerTests
    {
        private readonly Mock<IFinancialYearMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFinancialYearMasterQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper>   _mockMapper   = new(MockBehavior.Loose);

        private UpdateFinancialYearMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1, bool isLinked = false)
        {
            _mockQueryRepo.Setup(r => r.IsFinancialYearLinkedAsync(It.IsAny<int>())).ReturnsAsync(isLinked);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.FinancialYearMaster>(It.IsAny<UpdateFinancialYearMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.FinancialYearMaster());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>()))
                .ReturnsAsync(updatedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(updatedId: 5);
            var result = await CreateSut().Handle(FinancialYearMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_InactiveAndNotLinked_AllowsUpdate()
        {
            SetupHappyPath(isLinked: false);
            var cmd = FinancialYearMasterBuilders.ValidUpdateCommand(isActive: 0);

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InactiveAndLinked_ThrowsExceptionRules()
        {
            SetupHappyPath(isLinked: true);
            var cmd = FinancialYearMasterBuilders.ValidUpdateCommand(isActive: 0);

            Func<Task> act = () => CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*");
        }

        [Fact]
        public async Task Handle_ActiveCommand_DoesNotCheckLinked()
        {
            SetupHappyPath();
            var cmd = FinancialYearMasterBuilders.ValidUpdateCommand(isActive: 1);

            await CreateSut().Handle(cmd, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.IsFinancialYearLinkedAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(FinancialYearMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(FinancialYearMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "FINANCIAL_YEAR_MASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
