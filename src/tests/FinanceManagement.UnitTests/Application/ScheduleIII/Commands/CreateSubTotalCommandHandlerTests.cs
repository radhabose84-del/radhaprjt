using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class CreateSubTotalCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateSubTotalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateSubTotalCommand ValidCommand() =>
            new()
            {
                StructureId = 1,
                SubTotalName = "Gross Margin",
                IncludeOtherIncome = false,
                DisplayOrder = 5,
                Formulas = new List<SubTotalFormulaInput>
                {
                    new() { OperandTypeId = 140, OperandRefId = 19, OperatorId = 130, DisplayOrder = 1 },
                    new() { OperandTypeId = 140, OperandRefId = 22, OperatorId = 131, DisplayOrder = 2 }
                }
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(It.IsAny<CreateSubTotalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ScheduleIIISubTotal());
            _mockCommandRepo
                .Setup(r => r.CreateSubTotalAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(),
                    It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessAndId()
        {
            SetupHappyPath(newId: 7);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesAllFormulaOperandsToRepo()
        {
            List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>? captured = null;
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(It.IsAny<CreateSubTotalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.ScheduleIIISubTotal());
            _mockCommandRepo
                .Setup(r => r.CreateSubTotalAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>(),
                    It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .Callback<FinanceManagement.Domain.Entities.ScheduleIIISubTotal, List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>(
                    (_, f) => captured = f)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_SUBTOTAL_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
