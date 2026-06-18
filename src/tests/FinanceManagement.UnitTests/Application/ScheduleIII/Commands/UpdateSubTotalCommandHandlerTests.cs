using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class UpdateSubTotalCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateSubTotalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private static UpdateSubTotalCommand ValidCommand() =>
            new()
            {
                Id = 2,
                FormulaName = "EBITDA",
                IncludeOtherIncome = true,
                Formulas = new List<SubTotalFormulaInput>
                {
                    new() { OperandTypeId = 141, SectionItemId = 1, OperatorId = 130, DisplayOrder = 1 }
                }
            };

        private void SetupHappyPath(int result = 2)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateSubTotalAsync(
                    It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(),
                    It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .ReturnsAsync(result);
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
        public async Task Handle_ValidCommand_PassesIncludeOtherIncomeToRepo()
        {
            bool? capturedInclude = null;
            _mockCommandRepo
                .Setup(r => r.UpdateSubTotalAsync(
                    It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>(),
                    It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .Callback<int, string?, bool, List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>(
                    (_, _, inc, _) => capturedInclude = inc)
                .ReturnsAsync(2);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            capturedInclude.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesUpdateAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_SUBTOTAL_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
