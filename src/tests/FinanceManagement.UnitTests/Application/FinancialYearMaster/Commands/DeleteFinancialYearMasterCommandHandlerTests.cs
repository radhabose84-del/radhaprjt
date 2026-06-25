using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Commands
{
    public sealed class DeleteFinancialYearMasterCommandHandlerTests
    {
        private readonly Mock<IFinancialYearMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteFinancialYearMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteFinancialYearMasterCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSoftDeleteOnce()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteFinancialYearMasterCommand(7), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteFinancialYearMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "FINANCIAL_YEAR_MASTER_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
