using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class SaveSubTotalFormulaCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private SaveSubTotalFormulaCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private static SaveSubTotalFormulaCommand ValidCommand() =>
            new()
            {
                SubTotalId = 2,
                Formulas = new List<SubTotalFormulaInput>
                {
                    new() { OperandTypeId = 27, SectionItemId = null, OperandSubTotalId = 1, OperatorId = 24, DisplayOrder = 1 }, // sub-total operand
                    new() { OperandTypeId = 26, SectionItemId = 17, OperandSubTotalId = null, OperatorId = 24, DisplayOrder = 2 }  // line operand
                }
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockCommandRepo
                .Setup(r => r.SaveSubTotalFormulaAsync(2, It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .ReturnsAsync(2);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsOperandSubTotalIdAndSectionItemId()
        {
            List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>? captured = null;
            _mockCommandRepo
                .Setup(r => r.SaveSubTotalFormulaAsync(2, It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .Callback<int, List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>((_, f) => captured = f)
                .ReturnsAsync(2);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured.Should().HaveCount(2);
            captured!.Should().ContainSingle(f => f.OperandSubTotalId == 1 && f.SectionItemId == null);
            captured!.Should().ContainSingle(f => f.SectionItemId == 17 && f.OperandSubTotalId == null);
            captured!.Should().OnlyContain(f => f.SubTotalId == 2);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SaveSubTotalFormulaAsync(2, It.IsAny<List<FinanceManagement.Domain.Entities.ScheduleIIISubTotalFormula>>()))
                .ReturnsAsync(2);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_SUBTOTAL_FORMULA_SAVE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
